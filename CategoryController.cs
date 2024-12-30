using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Configuration;
using mvcADONET.Models;

namespace mvcADONET.Controllers
{
    public class CategoryController : Controller
    {


        private string connectionString = "Data Source=DESKTOP-OE9L8AE;Initial Catalog=xyz1;Integrated Security=True";

        // Display all categories
        public ActionResult Index()
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
            return View(categories);
        }

        // Create Category
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Category category)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO Categories (CategoryName) VALUES (@CategoryName)";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CategoryName", category.CategoryName);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
            return RedirectToAction("Index");
        }

        // Edit Category
        public ActionResult Edit(int id)
        {
            Category category = GetCategoryById(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // POST: Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    UpdateCategory(category);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    // Log the exception (you can use a logger here)
                    ModelState.AddModelError("", "An error occurred while saving the category: " + ex.Message);
                }
            }
            return View(category);
        }

        private Category GetCategoryById(int id)
        {
            Category category = null;
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "SELECT * FROM Categories WHERE CategoryId = @CategoryId";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@CategoryId", id);

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        category = new Category
                        {
                            CategoryId = Convert.ToInt32(reader["CategoryId"]),
                            CategoryName = reader["CategoryName"].ToString()
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error (you can use a logger here)
            }
            return category;
        }

        private void UpdateCategory(Category category)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "UPDATE Categories SET CategoryName = @CategoryName WHERE CategoryId = @CategoryId";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@CategoryName", category.CategoryName);
                    cmd.Parameters.AddWithValue("@CategoryId", category.CategoryId);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logger here)
                throw new Exception("Error updating category", ex);
            }
        }
        // Delete Category
        public ActionResult Delete(int id)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM Categories WHERE CategoryId = @CategoryId";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CategoryId", id);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
            return RedirectToAction("Index");
        }
    }
}