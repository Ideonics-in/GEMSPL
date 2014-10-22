using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using System.Configuration.Assemblies;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Xml;
using System.Threading;
using System.Windows.Threading;


using GEMSPL.Entity;
using GEMSPL.DashBoard;

namespace GEMSPL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        String BaseDirectory = string.Empty;
        Users Users;
        PortSettings Settings;
        public MainWindow()
        {
            InitializeComponent();


            BaseDirectory = @""+ConfigurationManager.AppSettings["BASE_DIR"];
            BaseDirectory = System.IO.Path.Combine(BaseDirectory + @"\GEMSPL");

            Users =  GetUsers(BaseDirectory);
            Settings = GetPortSetting(BaseDirectory);
            tbName.Focus();
        }

        private Users GetUsers(string BaseDirectory)
        {
            
            try
            {
                Users users;
                String file =System.IO.Path.Combine(BaseDirectory + @"\Users");
                FileStream fs;
                DataContractSerializer serializer = new DataContractSerializer(typeof(Users));
                if (!Directory.Exists(BaseDirectory))
                {
                    Directory.CreateDirectory(BaseDirectory);
                    
                     fs = File.Create(file);
                    
                    User admin = new User() { Name = "admin", Password = "admin123" };
                    users = new Users();
                    users.UserDictionary.Add("admin","admin123");
                    using (var sw = new StringWriter())
                    {

                        using (var writer = new XmlTextWriter(sw))
                        {
                            writer.Formatting = Formatting.Indented; // indent the Xml so it's human readable
                            serializer.WriteObject(writer,users);
                            writer.Flush();
                            String xmlString = sw.ToString();
                            using (var swriter = new StreamWriter(fs))
                            {
                                swriter.Write(xmlString);
                            }
                        
                        }
                    }
                }

                fs = new FileStream(file, FileMode.Open);
                XmlDictionaryReader reader =
                    XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());

                // Create the DataContractSerializer instance.
                DataContractSerializer ser =
                    new DataContractSerializer(typeof(Users));
                users = (Users)ser.ReadObject(reader);
                
                return users;
            }

            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        private PortSettings GetPortSetting(string BaseDirectory)
        {

            try
            {
                StringBuilder sb = new StringBuilder();
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent=true;
                settings.Encoding = Encoding.UTF8;
                settings.CloseOutput = false;
                settings.CheckCharacters = true;

                XmlWriter w = XmlWriter.Create(sb, settings);
                String file = System.IO.Path.Combine(BaseDirectory + @"\PortSettings");
                FileStream fs;
                XmlSerializer serializer = new XmlSerializer(typeof(PortSettings));
                
                
                PortSettings Settings;
                if (!File.Exists(file))
                {
                    

                    fs = File.Create(file);

                    Settings = new PortSettings("COM1", "9600", "NONE");
                    Encoding encoding = Encoding.GetEncoding("UTF-8");

                    using (StreamWriter sw = new StreamWriter(fs,encoding))
                    {
                        serializer.Serialize(sw,Settings);
                    }
                }

                
                StreamReader stream = new StreamReader(file);
                Settings = (PortSettings)serializer.Deserialize(stream);
                stream.Close();
                
                return Settings;
            }

            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        private void btnLogin_Click_1(object sender, RoutedEventArgs e)
        {
            if (Users.UserDictionary.ContainsKey(tbName.Text))
            {
                if (Users.UserDictionary[tbName.Text] == tbPassword.Password)
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                 new Action(() =>
                                 {
                                     BaseGrid.Children.Clear();
                                     this.WindowState = System.Windows.WindowState.Maximized;

                                     DashBoardView dbView = new DashBoardView(BaseDirectory,Users,tbName.Text,Settings);

                                     if (tbName.Text != "admin")
                                     {
                                         dbView.AddULBContextMenu.Visibility = System.Windows.Visibility.Collapsed;
                                         dbView.PortSetting.Visibility = System.Windows.Visibility.Collapsed;
                                         dbView.UsersButton.Visibility = System.Windows.Visibility.Collapsed;
                                         dbView.Admin = false;


                                     }
                                     else
                                         dbView.Admin = true;

                                     this.BaseGrid.Children.Add(dbView);

                                 }));
                }
                else
                {
                    MessageBox.Show("Incorrect Password", "Login Failure", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                  new Action(() =>
                                  {
                                      tbName.Clear();
                                      tbPassword.Clear();
                                      tbName.Focus();


                                  }));
                }
            }
        }
    }
}
