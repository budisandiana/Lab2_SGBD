using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace Lab2
{
    public partial class Form1 : Form
    {        

        public Form1()
        {
            //initializeaza aplicatia (windows forms)
            InitializeComponent();

            //la inceput setam ca butoanele sa nu poata fi apasate pana nu este apasat butonul open
            insertButton.Enabled = false;
            updateButton.Enabled = false;
            deleteButton.Enabled = false;
        }


        private void openButton_Click(object sender, EventArgs e)
        {
            //connectionString => se conecteaza la baza de date Hospital
            string str = "Data Source= .\\SQLEXPRESS;Initial Catalog=Hospital;Integrated Security=True;MultipleActiveResultSets=True";
            SqlConnection connection = new SqlConnection(str);
            SqlDataAdapter adapter; //bridge between DataSet and Sql Server => preia si salveaza datele

            string illnessQuery = "select * from illness";
            string diagnosticQuery = "select * from Diagnostic";
            
            //create a DataSet
            DataSet datas1 = new DataSet();

            try
            {                
                connection.Open(); //deschide conexiunea 
                adapter = new SqlDataAdapter(illnessQuery, connection);
                adapter.Fill(datas1, "illness"); //adapter.Fill schimba datele din DataSet ca sa fie egale cu datele din tabelul dat (illness)
                adapter = new SqlDataAdapter(diagnosticQuery, connection);
                adapter.Fill(datas1, "Diagnostic"); //adapter.Fill schimba datele din DataSet ca sa fie egale cu datele din tabelul dat (Diagnostic)

                //facem legatura dintre illness (tabelul parinte) si Diagnostic (tabelul copil)
                datas1.Relations.Add("illness Diagnostic",
                    datas1.Tables["illness"].Columns["illness_ID"],
                    datas1.Tables["Diagnostic"].Columns["illness_ID"]);
                connection.Close(); //inchidem conexiunea

                dataGridView1.DataSource = datas1.Tables["illness"];
            }

            //daca nu reuseste sa faca conexiunea cu baza de date, atunci va afisa eroare
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                connection.Close();
            }
        }

        //functie ca sa ma ajute sa vad datele in timp real atunci cand fac insert/delete/update
        private void data_display()
        {
            //get the row number on the DataGrid
            int rownr = this.dataGridView1.CurrentCell.RowIndex;

            //get the content of the cell with the Id
            object cellvalue = this.dataGridView1[0, rownr].Value; //luam prima coloana din tabel, care e illness_ID
            string idString = cellvalue.ToString();
            illnessTextBox.Text = idString; //va aparea id-ul in primul textBox
            int id = Int32.Parse(idString);

            //create connection
            string str = "Data Source= .\\SQLEXPRESS;Initial Catalog=Hospital;Integrated Security=True;MultipleActiveResultSets=True";
            SqlConnection connection = new SqlConnection(str);

            //create the command and parameter objects
            string childQuery = "select * from Diagnostic where illness_ID = @illness_ID";
            SqlCommand cmd = new SqlCommand(childQuery, connection); //se conecteaza child table-ul si parent table
            cmd.Parameters.Add("@illness_ID", SqlDbType.Int);
            cmd.Parameters["@illness_ID"].Value = id;

            try
            {
                connection.Open(); //deschidem conexiunea
                SqlDataReader reader = cmd.ExecuteReader(); //citeste datele
                DataTable childTable = new DataTable();
                childTable.Load(reader);
                dataGridView2.DataSource = childTable; //afiseaza in DataGridView2 child table-ul
                reader.Close(); //inchide conexiunea
            }

            //daca nu reuseste sa faca conexiunea cu baza de date, atunci va afisa eroare
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            data_display(); //apelam functia pentru a face conexiunea la baza de date si a afisa tabelul parinte in DataGridView1

            insertButton.Enabled = true; //pentru ca avem id-ul din tabelul parinte (illness) deja in textbox (si este setat la readOnly), 
                                         //putem sa inseram in tabelul copil (diagnostic) doar valori cu id-ul selectat
            updateButton.Enabled = false; //celelalte butoane raman oprite pana apasam pe id-ul din tabelul copil
            deleteButton.Enabled = false;
            illnessTextBox.ReadOnly = true;
        }

        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //get the row number on the DataGrid
            int rowNr = this.dataGridView2.CurrentCell.RowIndex;

            //get the content of the cells
            object cellvalue0 = this.dataGridView2[0, rowNr].Value;
            object cellvalue1 = this.dataGridView2[1, rowNr].Value;
            object cellvalue2 = this.dataGridView2[2, rowNr].Value;

            //fiecare textbox este asociat cu cate o coloana din tabel
            diagnosticTextBox.Text = cellvalue0.ToString();
            illnessTextBox.Text = cellvalue1.ToString();
            simptomsTextBox.Text = cellvalue2.ToString();

            //pentru ca toate datele sunt completate in textbox-uri, acum putem sa facem update si delete
            deleteButton.Enabled = true;
            updateButton.Enabled = true;
            insertButton.Enabled = true;
            illnessTextBox.ReadOnly = false;
        }


        private void deleteButton_Click(object sender, EventArgs e)
        {
            //get the row number on the DataGrid
            int rowNr = this.dataGridView2.CurrentCell.RowIndex;

            //get the content of the cells
            object cellvalue0 = this.dataGridView2[0, rowNr].Value;
            string idString = cellvalue0.ToString();
            diagnosticTextBox.Text = idString;
            int id = Int32.Parse(idString);


            //connectionString
            string str = "Data Source= .\\SQLEXPRESS;Initial Catalog=Hospital;Integrated Security=True;MultipleActiveResultSets=True";
            SqlConnection connection = new SqlConnection(str);
            SqlDataAdapter adapter = new SqlDataAdapter();

            //create the command and parameter objects
            string deleteQuery = "delete from Diagnostic where Diagnostic_ID= @Diagnostic_ID";
            adapter.DeleteCommand = new SqlCommand(deleteQuery, connection);
            adapter.DeleteCommand.Parameters.Add("@Diagnostic_ID", SqlDbType.Int);
            adapter.DeleteCommand.Parameters["@Diagnostic_ID"].Value = id;

            try
            {
                connection.Open();
                adapter.DeleteCommand.ExecuteNonQuery();
                data_display(); //apelam functia ca atunci cand face delete, sa afiseze din nou tabelul cu datele noi
                connection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                connection.Close();
            }

            //resetam datele din textbox-uri
            illnessTextBox.Text = ""; 
            diagnosticTextBox.Text = "";
            simptomsTextBox.Text = "";

        }

        private void updateButton_Click(object sender, EventArgs e)
        {
            //get the row number on the DataGrid
            int rowNr = this.dataGridView2.CurrentCell.RowIndex;

            //get the content of the cells
            object cellvalue0 = this.dataGridView2[0, rowNr].Value;
            string idString = cellvalue0.ToString();
            diagnosticTextBox.Text = idString;

            //connectionString
            string str = "Data Source= .\\SQLEXPRESS;Initial Catalog=Hospital;Integrated Security=True;MultipleActiveResultSets=True";
            SqlConnection connection = new SqlConnection(str);
            SqlDataAdapter adapter = new SqlDataAdapter();

            //create the command and parameter objects
            string updateQuery = "update Diagnostic set Diagnostic_ID = @Diagnostic_ID, illness_ID = @illness_ID, Simptoms = @Simptoms where Diagnostic_ID = @newID";
            adapter.UpdateCommand = new SqlCommand(updateQuery, connection);
            adapter.UpdateCommand.Parameters.Add("@newID", SqlDbType.Int);
            adapter.UpdateCommand.Parameters["@newID"].Value = cellvalue0.ToString();

            adapter.UpdateCommand.Parameters.Add("@Simptoms", SqlDbType.VarChar, 30);
            adapter.UpdateCommand.Parameters["@Simptoms"].Value = simptomsTextBox.Text;
            try
            {
                adapter.UpdateCommand.Parameters.Add("@Diagnostic_ID", SqlDbType.Int);
                adapter.UpdateCommand.Parameters["@Diagnostic_ID"].Value = Int32.Parse(diagnosticTextBox.Text);

                adapter.UpdateCommand.Parameters.Add("@illness_ID", SqlDbType.Int);
                adapter.UpdateCommand.Parameters["@illness_ID"].Value = Int32.Parse(illnessTextBox.Text);

                connection.Open();
                adapter.UpdateCommand.ExecuteNonQuery();
                data_display(); //apelam functia ca atunci cand face update, sa afiseze din nou tabelul cu datele noi
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                connection.Close();
            }

            illnessTextBox.Text = "";
            diagnosticTextBox.Text = "";
            simptomsTextBox.Text = "";
        }

        private void insertButton_Click(object sender, EventArgs e)
        {

            //connectionString
            string str = "Data Source= .\\SQLEXPRESS;Initial Catalog=Hospital;Integrated Security=True;MultipleActiveResultSets=True";
            SqlConnection connection = new SqlConnection(str);
            SqlDataAdapter adapter = new SqlDataAdapter();


            //create the command and parameter objects
            string insertQuery = "insert into Diagnostic values(@Diagnostic_ID, @illness_ID, @Simptoms)";
            adapter.InsertCommand = new SqlCommand(insertQuery, connection);
            adapter.InsertCommand.Parameters.Add("@illness_ID", SqlDbType.Int);
            adapter.InsertCommand.Parameters["@illness_ID"].Value = Int32.Parse(illnessTextBox.Text);
            adapter.InsertCommand.Parameters.Add("@Simptoms", SqlDbType.VarChar, 30);
            adapter.InsertCommand.Parameters["@Simptoms"].Value = simptomsTextBox.Text;

            try
            {
                adapter.InsertCommand.Parameters.Add("@Diagnostic_ID", SqlDbType.Int);
                adapter.InsertCommand.Parameters["@Diagnostic_ID"].Value = Int32.Parse(diagnosticTextBox.Text);

                connection.Open();
                adapter.InsertCommand.ExecuteNonQuery();
                data_display(); //apelam functia ca atunci cand face insert, sa afiseze din nou tabelul cu datele noi
                connection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                connection.Close();
            }

            diagnosticTextBox.Text = "";
            simptomsTextBox.Text = "";
        }

    }
}
