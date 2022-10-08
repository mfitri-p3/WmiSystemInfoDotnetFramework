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
using System.Management;

namespace WmiSystemInfoWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DetailsListView.Items.Clear();
            GetBatteryInfo();
        }

        public void GetBatteryInfo()
        {
            //Reference: https://stackoverflow.com/questions/8945986/find-out-battery-charge-capacity-in-percentage-using-c-sharp-or-net

            System.Management.ObjectQuery query = new ObjectQuery("Select * FROM Win32_Battery");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);

            ManagementObjectCollection collection = searcher.Get();

            foreach (ManagementObject mo in collection)
            {
                foreach (PropertyData property in mo.Properties)
                {
                    //Console.WriteLine("Property {0}: Value is {1}", property.Name, property.Value);
                    if (property.Value != null)
                    {
                        if (property.IsArray)
                        {
                            //PowerManagementCapabilities property will land here
                            string strProps = "";
                            try
                            {
                                var arrVal = property.Value;
                                foreach (var subVal in (Array)arrVal)
                                {
                                    strProps += subVal;
                                }
                            }
                            catch (Exception ex)
                            {
                                strProps = "Error: " + ex.Message;
                            }
                            DetailsListView.Items.Add(string.Format("{0}: {1}", property.Name, strProps));
                        }
                        else
                        {
                            DetailsListView.Items.Add(string.Format("{0}: {1}", property.Name, ParseValue(property.Name, property.Value, property.Type)));
                        }
                    }
                }
            }
        }

        private string ParseValue(string name, object val, CimType type)
        {
            if (val != null)
            {
                switch (type)
                {
                    case CimType.None:
                        return "NULL";
                    case CimType.SInt8:
                    case CimType.UInt8:
                    case CimType.SInt16:
                    case CimType.UInt16:
                    case CimType.SInt32:
                    case CimType.UInt32:
                    case CimType.SInt64:
                    case CimType.UInt64:
                        if (name.Contains("Time"))
                        {
                            //Reference: https://stackoverflow.com/questions/249760/how-can-i-convert-a-unix-timestamp-to-datetime-and-vice-versa
                            DateTime parsedDt = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(double.Parse(val.ToString()));
                            TimeSpan remainingTs = parsedDt.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0));
                            return string.Format("{0} Hours", remainingTs.TotalHours.ToString("F2"));
                        }
                        else if (name.Contains("Voltage"))
                        {
                            return string.Format("{0} mV", val.ToString());
                        }
                        else if (name.Contains("ChargeRemaining"))
                        {
                            return string.Format("{0}%", val.ToString());
                        }
                        else if (name.Contains("Capacity"))
                        {
                            return string.Format("{0} mWHr", val.ToString());
                        }
                        else
                        {
                            return val.ToString();
                        }
                    case CimType.Real32:
                        break;
                    case CimType.Real64:
                        break;
                    case CimType.Boolean:
                        return val.ToString() == "False" ? "Yes" : "No";
                    case CimType.String:
                        return val.ToString();
                    case CimType.DateTime:
                        return DateTime.Parse(val.ToString()).ToString("yyyy-MM-dd");
                    case CimType.Reference:
                        return "REFERENCE VALUE";
                    case CimType.Char16:
                    case CimType.Object:
                    default:
                        return val.ToString();
                }
                return val.ToString();
            }
            else
            {
                return "NULL";
            }
        }
    }
}
