using BusinessObject;
using DataAccess.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyStoreWinApp
{
    public partial class frmMemberManagement : Form
    {
        public MemberObject loginMember { get; set; }
        IMemberRepository memberRepository = new MemberRepository();

        BindingSource source;
        BindingSource citySource;
        BindingSource countrySource;
        public frmMemberManagement()
        {
            InitializeComponent();
        }

        private void frmMemberManagement_Load(object sender, EventArgs e)
        {
            btnDelete.Enabled = false;
            txtMemberID.Enabled = false;
            txtMemberName.Enabled = false;
            txtEmail.Enabled = false;
            txtPassword.Enabled = false;
            txtCity.Enabled = false;
            txtCountry.Enabled = false;
        }

        private MemberObject GetMemberInfo()
        {
            MemberObject member = null;
            try
            {
                member = new MemberObject
                {
                    MemberID = int.Parse(txtMemberID.Text),
                    MemberName = txtMemberName.Text,
                    Email = txtEmail.Text,
                    Password = txtPassword.Text,
                    City = txtCity.Text,
                    Country = txtCountry.Text
                };
            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Get Member Info", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return member;
        }
        private void LoadMemberList(bool search = false, IEnumerable<MemberObject> searchDataSource = null)
        {
            var members = memberRepository.GetMembersList();
            var memberList = from member in members
                             orderby member.MemberName descending
                             select member;
            try
            {
                source = new BindingSource();
                if (search)
                {
                    source.DataSource = searchDataSource;
                } else
                {
                    source.DataSource = memberList;

                    var countryList = from member in members
                                      where !string.IsNullOrEmpty(member.Country.Trim())
                                      orderby member.Country ascending
                                      select member.Country;
                    countryList = countryList.Prepend("All");
                    countryList = countryList.Distinct();

                    var cityList = from member in members
                                   where !string.IsNullOrEmpty(member.City.Trim())
                                   orderby member.City ascending
                                   select member.City;
                    cityList = cityList.Prepend("All");
                    cityList = cityList.Distinct();

                    if (memberList.Count() > 0)
                    {
                        countrySource = new BindingSource();
                        countrySource.DataSource = countryList;
                        cboCountry.DataSource = null;
                        cboCountry.DataSource = countrySource;

                        citySource = new BindingSource();
                        citySource.DataSource = cityList;
                        cboSearchCity.DataSource = null;
                        cboSearchCity.DataSource = citySource;
                    }
                }

                txtMemberID.DataBindings.Clear();
                txtMemberName.DataBindings.Clear();
                txtEmail.DataBindings.Clear();
                txtPassword.DataBindings.Clear();
                txtCity.DataBindings.Clear();
                txtCountry.DataBindings.Clear();

                txtMemberID.DataBindings.Add("Text", source, "MemberID");
                txtMemberName.DataBindings.Add("Text", source, "MemberName");
                txtEmail.DataBindings.Add("Text", source, "Email");
                txtPassword.DataBindings.Add("Text", source, "Password");
                txtCity.DataBindings.Add("Text", source, "City");
                txtCountry.DataBindings.Add("Text", source, "Country");

                dgvMemberList.DataSource = null;
                dgvMemberList.DataSource = source;

                if (memberList.Count() > 0) 
                {
                    btnDelete.Enabled = true;
                } else
                {
                    btnDelete.Enabled = false;
                }

            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Load Member List");
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            LoadMemberList();
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            frmMemberDetails frmMemberDetails = new frmMemberDetails
            {
                memberRepository = this.memberRepository,
                InsertOrUpdate = true,
                Text = "Add new member"
            };

            if (frmMemberDetails.ShowDialog() == DialogResult.OK)
            {
                LoadMemberList();
                source.Position = source.Count - 1;
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            MemberObject member = GetMemberInfo();
            if (member.MemberID == loginMember.MemberID)
            {
                MessageBox.Show("You can't delete yourself!!", "Delete member", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } else
            {
                if (MessageBox.Show($"Do you really want to delete the member: \n" +
                $"Member ID: {member.MemberID}\n" +
                $"Member Name: {member.MemberName}\n" +
                $"Email: {member.Email}\n" +
                $"City: {member.City}\n" +
                $"Country: {member.Country}", "Delete member", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    memberRepository.DeleteMember(member.MemberID);
                    LoadMemberList();
                }
            }
            
        }

        private void dgvMemberList_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            MemberObject member = GetMemberInfo();

            frmMemberDetails frmMemberDetails = new frmMemberDetails
            {
                memberRepository = this.memberRepository,
                InsertOrUpdate = false,
                memberInfo = member,
                Text = "Update member info"
            };
            
            if (frmMemberDetails.ShowDialog() == DialogResult.OK)
            {
                LoadMemberList();
                source.Position = source.Count - 1;
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                string searchValue = txtSearchValue.Text;
                if (radioByID.Checked)
                {
                    int searchID = int.Parse(searchValue);
                    IEnumerable<MemberObject> searchResult = memberRepository.SearchMember(searchID);
                    if (searchResult.Any())
                    {
                        LoadMemberList(true, searchResult);
                    } else
                    {
                        MessageBox.Show("No result found!", "Search member", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    
                } 
                else if (radioByName.Checked)
                {
                    string searchName = searchValue;
                    IEnumerable<MemberObject> searchResult = memberRepository.SearchMember(searchName);
                    if (searchResult.Any())
                    {
                        LoadMemberList(true, searchResult);
                    }
                    else
                    {
                        MessageBox.Show("No result found!", "Search member", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Search member", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }

        private void cboCountry_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cboCountry.DataSource != null)
                {
                    string country = cboCountry.SelectedItem.ToString();
                    if (!string.IsNullOrEmpty(country))
                    {
                        IEnumerable<MemberObject> searchResult = memberRepository.SearchMemberByCountry(country);
                        if (searchResult.Any())
                        {
                            LoadMemberList(true, searchResult);
                        }
                        else
                        {
                            MessageBox.Show("No result found!", "Search member", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Search member", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cboSearchCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cboSearchCity.DataSource != null)
                {
                    string city = cboSearchCity.SelectedItem.ToString();
                    if (!string.IsNullOrEmpty(city))
                    {
                        IEnumerable<MemberObject> searchResult = memberRepository.SearchMemberByCity(city);
                        if (searchResult.Any())
                        {
                            LoadMemberList(true, searchResult);
                        }
                        else
                        {
                            MessageBox.Show("No result found!", "Search member", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Search member", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
