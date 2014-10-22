using GEMSPL.Entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GEMSPL.DashBoard.Manage;
using System.Runtime.Serialization;
using System.Xml;
using GEMSPL;
using System.Xml.Serialization;

namespace GEMSPL.DashBoard
{
    public partial class DashBoardView : UserControl
    {
        public ObservableCollection<ULB> ULBs { get; set; }
        public String CurrentUser { get; set; }
        Users Users;
        public String BaseDir { get; set; }

        public UIElement TansientElement { get; set; }


       

        void GetULBs()
        {
            ULBs = new ObservableCollection<ULB>();
            System.Windows.Visibility visibility = (CurrentUser== "admin") ? (Visibility.Visible) : Visibility.Collapsed;
            foreach (String s in Directory.GetDirectories(BaseDir))
            {

                ULBs.Add(new ULB(s, visibility));
            }
            GEMSPLTree.DataContext = ULBs;
        }



        #region MANAGE_PASSWORD
        private void Password_Click_1(object sender, RoutedEventArgs e)
        {
            PasswordControl pwdControl = new PasswordControl(Users.UserDictionary[CurrentUser]);
            pwdControl.PasswordChangeEvent += pwdControl_PasswordChangeEvent;
            pwdControl.PasswordCancelEvent += pwdControl_PasswordCancelEvent;

            Transient.Children.Clear();
            Transient.Children.Add(pwdControl);
            Keyboard.Focus(pwdControl);
        }

        void pwdControl_PasswordCancelEvent(object sender, PasswordCancelEventArgs e)
        {
            Transient.Children.Clear();
        }

        void pwdControl_PasswordChangeEvent(object sender, PasswordChangeEventArgs e)
        {
            Users.UserDictionary[CurrentUser] = e.Password;
            ChangeUsers();
            MessageBox.Show("Password Changed", "Application Info", MessageBoxButton.OK, MessageBoxImage.Information);
            Transient.Children.Clear();
        }

#endregion


        #region MANAGE_USERS
        private void Users_Click_1(object sender, RoutedEventArgs e)
        {
            UsersControl usersControl = new UsersControl(Users);
            usersControl.AddUserEvent += usersControl_AddUserEvent;
            usersControl.DeleteUserEvent += usersControl_DeleteUserEvent;
            usersControl.CancelEvent += usersControl_CancelEvent;
            Transient.Children.Clear();
            Transient.Children.Add(usersControl);

        }

        void usersControl_CancelEvent(object sender, GEMSPL.DashBoard.Manage.CancelEventArgs e)
        {
            Transient.Children.Clear();
        }

        void usersControl_DeleteUserEvent(object sender, DeleteUserEventArgs e)
        {
            Users.UserDictionary.Remove(e.Name);
            ChangeUsers();
            MessageBox.Show("User Deleted", "Application Info", MessageBoxButton.OK, MessageBoxImage.Information);
            Transient.Children.Clear();

        }

        void usersControl_AddUserEvent(object sender, AddUserEventArgs e)
        {
            Users.UserDictionary.Add(e.Name, e.Password);
            ChangeUsers();
            MessageBox.Show("User Added", "Application Info", MessageBoxButton.OK, MessageBoxImage.Information);
            Transient.Children.Clear();
        }

        void ChangeUsers()
        {
            try
            {

                String file = System.IO.Path.Combine(BaseDir + @"\Users");
                FileStream fs;
                DataContractSerializer serializer = new DataContractSerializer(typeof(Users));


                fs = new FileStream(file, FileMode.Truncate);


                using (var sw = new StringWriter())
                {

                    using (var writer = new XmlTextWriter(sw))
                    {
                        writer.Formatting = Formatting.Indented; // indent the Xml so it's human readable
                        serializer.WriteObject(writer, Users);
                        writer.Flush();
                        String xmlString = sw.ToString();
                        using (var swriter = new StreamWriter(fs))
                        {
                            swriter.Write(xmlString);
                        }

                    }

                }


            }

            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }


        }

        #endregion


        #region MANAGE_SETTINGS
        private void PortSetting_Click_1(object sender, RoutedEventArgs e)
        {
            PortSetttingsControl psControl = new PortSetttingsControl(PortSettings);
            
            psControl.SaveEvent += psControl_SaveEvent;
            psControl.CancelEvent += psControl_CancelEvent;
            Transient.Children.Clear();
            Transient.Children.Add(psControl);

        }

        void psControl_CancelEvent(object sender, GEMSPL.DashBoard.Manage.CancelEventArgs e)
        {
            Transient.Children.Clear();
        }

        void psControl_SaveEvent(object sender, SaveEventArgs e)
        {
            PortSettings = e.Settings;
            ChangeSettings();
            MessageBox.Show("Settings Changed"+Environment.NewLine+"Please Restart Application for new values to take effect", 
                "Application Info", MessageBoxButton.OK, MessageBoxImage.Information);

        }

        private void ChangeSettings()
        {
            
            String file = System.IO.Path.Combine(BaseDir +"\\"+"PortSettings");
            FileStream fs;
            XmlSerializer serializer = new XmlSerializer(typeof(PortSettings));

            fs = File.OpenWrite(file);
        
                Encoding encoding = Encoding.GetEncoding("UTF-8");

                using (StreamWriter sw = new StreamWriter(fs, encoding))
                {
                    serializer.Serialize(sw, PortSettings);
                }
                fs.Close();
            
            
        }
        #endregion

        private void UserControl_Loaded_1(object sender, RoutedEventArgs e)
        {

        }



      





      
    }
}
