using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using mvcADONET.Models;

public class ProductController : Controller
{
    private string connectionString = "Data Source=DESKTOP-OE9L8AE;Initial Catalog=xyz1;Integrated Security=True";

    // Product List with Pagination

    public ActionResult Index(int page = 1)
    {
        int pageSize = 10;
        List<Product> products = new List<Product>();

        using (SqlConnection con = new SqlConnection(connectionString))
        {
            string query = "SELECT p.ProductId, p.ProductName, p.CategoryId, c.CategoryName " +
                           "FROM Products p " +
                           "JOIN Categories c ON p.CategoryId = c.CategoryId " +
                           "ORDER BY p.ProductId " +
                           "OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);

            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                products.Add(new Product
                {
                    ProductId = Convert.ToInt32(reader["ProductId"]),
                    ProductName = reader["ProductName"].ToString(),
                    CategoryId = Convert.ToInt32(reader["CategoryId"]),
                    CategoryName = reader["CategoryName"].ToString()
                });
            }
            con.Close();
        }

        ViewBag.Page = page;
        return View(products);
    }

    // Create Product
    [HttpGet]
    public ActionResult Create()
    {
        List<Category> categories = new List<Category>();
        using (SqlConnection con = new SqlConnection(connectionString))
        {
            string query = "SELECT * FROM Categories";
            SqlCommand cmd = new SqlCommand(query, con);
            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                categories.Add(new Category
                {
                    CategoryId = Convert.ToInt32(reader["CategoryId"]),
                    CategoryName = reader["CategoryName"].ToString()
                });
            }
            con.Close();
        }

        // Convert List<Category> to SelectList
        ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");

        return View();
    }


    [HttpPost]
    public ActionResult Create(Product product)
    {
        using (SqlConnection con = new SqlConnection(connectionString))
        {
            string query = "INSERT INTO Products (ProductName, CategoryId) VALUES (@ProductName, @CategoryId)";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@ProductName", product.ProductName);
            cmd.Parameters.AddWithValue("@CategoryId", product.CategoryId);
            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();
        }
        return RedirectToAction("Index");
    }

    // Edit Product
    [HttpGet]
    public ActionResult Edit(int id)
    {
        Product product = GetProductById(id);
        if (product == null)
        {
            return HttpNotFound();
        }

        List<Category> categories = GetCategories();
        ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName", product.CategoryId);
        return View(product);
    }

    // POST: Product/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(Product product)
    {
        if (ModelState.IsValid)
        {
            bool success = UpdateProduct(product);
            if (success)
            {
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "Failed to update the product. Please try again.");
            }
        }

        // If the model state is invalid or the update failed, re-render the Edit form
        List<Category> categories = GetCategories();
        ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName", product.CategoryId);
        return View(product);
    }

    private Product GetProductById(int id)
    {
        Product product = null;
        try
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Products WHERE ProductId = @ProductId";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ProductId", id);

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    product = new Product
                    {
                        ProductId = Convert.ToInt32(reader["ProductId"]),
                        ProductName = reader["ProductName"].ToString(),
                        CategoryId = Convert.ToInt32(reader["CategoryId"])
                    };
                }
            }
        }
        catch (Exception ex)
        {
            // Log the error (for debugging purposes)
            Console.WriteLine("Error retrieving product: " + ex.Message);
        }
        return product;
    }

    private List<Category> GetCategories()
    {
        List<Category> categories = new List<Category>();
        try
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Categories";
                SqlCommand cmd = new SqlCommand(query, con);

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    categories.Add(new Category
                    {
                        CategoryId = Convert.ToInt32(reader["CategoryId"]),
                        CategoryName = reader["CategoryName"].ToString()
                    });
                }
            }
        }
        catch (Exception ex)
        {
            // Log the error (for debugging purposes)
            Console.WriteLine("Error retrieving categories: " + ex.Message);
        }
        return categories;
    }

    private bool UpdateProduct(Product product)
    {
        try
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "UPDATE Products SET ProductName = @ProductName, CategoryId = @CategoryId WHERE ProductId = @ProductId";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ProductName", product.ProductName);
                cmd.Parameters.AddWithValue("@CategoryId", product.CategoryId);
                cmd.Parameters.AddWithValue("@ProductId", product.ProductId);

                con.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }
        catch (Exception ex)
        {
            // Log the error (for debugging purposes)
            Console.WriteLine("Error updating product: " + ex.Message);
            return false;
        }
    }
    // Delete Product
    public ActionResult Delete(int id)
    {
        using (SqlConnection con = new SqlConnection(connectionString))
        {
            string query = "DELETE FROM Products WHERE ProductId = @ProductId";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@ProductId", id);
            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();
        }
        return RedirectToAction("Index");
    }
}
