using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace PhoneBook
{
    public partial class MainForm : Form
    {
        private BindingSource bindingSourceControl;

        public MainForm()
        {
            InitializeComponent();

            // Initialize BindingSource
            bindingSourceControl = new BindingSource();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                // Reload the dataset from the database
                ReloadData();

                // Bind BindingSource to the dataset
                bindingSourceControl.DataSource = personDataSet.Person;

                // Bind DataGridView to BindingSource
                dataGridViewPersons.DataSource = bindingSourceControl;

                // Bind TextBoxes to BindingSource
                nameTextBox.DataBindings.Add("Text", bindingSourceControl, "Name");
                phoneTextBox.DataBindings.Add("Text", bindingSourceControl, "Phone Number");

                // Disable adding new rows directly in the DataGridView
                dataGridViewPersons.AllowUserToAddRows = false;

                // Hide the PersonID column in DataGridView
                if (dataGridViewPersons.Columns["PersonID"] != null)
                {
                    dataGridViewPersons.Columns["PersonID"].Visible = false;
                }

                // Initial sort by last name
                SortByLastName();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Check for unsaved changes
            if (personDataSet.Person.GetChanges() != null)
            {
                var result = MessageBox.Show(
                    "You have unsaved changes. Would you like to save them before exiting?",
                    "Unsaved Changes",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    btnSave_Click(null, null); // Save changes
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true; // Cancel the form close
                }
                else
                {
                    ReloadData(); // Discard changes
                }
            }
        }

        private void ReloadData()
        {
            try
            {
                // Refill the dataset to discard any unsaved changes
                this.personTableAdapter.Fill(this.personDataSet.Person);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reloading data: {ex.Message}");
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                // Get the next Id for the new row
                int nextId = personDataSet.Person.AsEnumerable()
                              .Select(row => row.Field<int>("PersonID"))
                              .DefaultIfEmpty(0)
                              .Max() + 1;

                // Add a blank row to the dataset
                DataRow newRow = personDataSet.Person.NewRow();
                newRow["PersonID"] = nextId;
                newRow["Name"] = ""; // Leave Name blank
                newRow["Phone Number"] = ""; // Leave Phone Number blank
                personDataSet.Person.Rows.Add(newRow);

                // Position the BindingSource to the new row
                bindingSourceControl.Position = bindingSourceControl.Count - 1;

                // Set focus to the Name TextBox for immediate editing
                nameTextBox.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding contact: {ex.Message}");
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (bindingSourceControl.Current != null)
                {
                    // Remove the selected row from the dataset
                    bindingSourceControl.RemoveCurrent();
                }
                else
                {
                    MessageBox.Show("No contact selected to delete.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting contact: {ex.Message}");
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveChanges();
        }

        private void SaveChanges()
        {
            try
            {
                // Commit pending changes in the BindingSource to the DataTable
                bindingSourceControl.EndEdit();

                // Save changes to the database
                int rowsAffected = personTableAdapter.Update(personDataSet.Person);

                // Notify the user
                MessageBox.Show($"{rowsAffected} changes saved successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving data: {ex.Message}");
            }
        }

        private void SortByLastName()
        {
            try
            {
                DataTable table = personDataSet.Person;

                // Clone the original table structure
                DataTable sortedTable = table.Clone();

                // Sort rows by the first letter of the last name
                var sortedRows = table.AsEnumerable()
                    .OrderBy(row =>
                    {
                        string name = row.Field<string>("Name") ?? string.Empty;
                        var parts = name.Split(' ');
                        return parts.Length > 1 ? parts[1] : name;
                    });

                // Import sorted rows into the cloned table
                foreach (var row in sortedRows)
                {
                    sortedTable.ImportRow(row);
                }

                // Clear the original table and re-import sorted rows
                table.Clear();
                foreach (DataRow row in sortedTable.Rows)
                {
                    table.ImportRow(row);
                }

                // Rebind the updated table to the BindingSource
                bindingSourceControl.DataSource = table;

                // Ensure DataGridView reflects the new binding
                dataGridViewPersons.DataSource = bindingSourceControl;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sorting data: {ex.Message}");
            }
        }
    }
}
