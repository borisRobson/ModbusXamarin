using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using Xamarin.Forms;

using System.Diagnostics;

namespace ModbusXamarin
{

    public partial class ControlPage : ContentPage
    {
        //Boolean checks for Live Polling
        public bool isLive;
        public bool isReady = true;
        public class Outputs
        {
            public string out1 { get; set; }
            public string out2 { get; set; }
            public string out3 { get; set; }
            public string out4 { get; set; }
        }
        public class Data
        {
            public string ip { get; set; }
            public string devNum { get; set; }
            public int data { get; set; }

        }
        
        public ControlPage()
        {
            InitializeComponent();
        }        

        //Set output states
        void ToggleState(object sender, EventArgs e)
        {
            //Access UI on main thread
            Device.BeginInvokeOnMainThread(() =>
            {
                Button button = (Button)sender;
                if (button.Text == "Off")
                {
                    button.Text = "On";
                    button.BackgroundColor = Color.Green;
                }
                else
                {
                    button.Text = "Off";
                    button.BackgroundColor = Color.Red;
                }
            });
        }

        //isReady to false to ensure that one is not exectued before the other replies
        void GetStates(object sender, EventArgs e)
        {
            isReady = false;
            Device.BeginInvokeOnMainThread(() =>
            {
                btnRead.BackgroundColor = Color.Green;
            });
            readData();
        }

        void SetOutputs(object sender, EventArgs e)
        {
            isReady = false;
            Device.BeginInvokeOnMainThread(() =>
            {
                btnWrite.BackgroundColor = Color.Green;
            });
            setOutputs();
        }

        void readData()
        {
            var host = hostIpAddr.Text;
            Data data = new Data();
            data.ip = devIpAddr.Text;
            data.devNum = devNum.Text;
            var uriString = string.Format("http://{0}:3000/ismaView/control/readInputs?ip={1}&devNum={2}", host, data.ip, data.devNum);
            Uri uri = new Uri(uriString);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);
            req.Method = "GET";
            try
            {
                req.BeginGetResponse(getResponseHandler, req);                
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ex: " + ex);
            }
        }

        void setOutputs()
        {
            var out1 = (Out1.Text == "On") ? "on" : "off";
            var out2 = (Out2.Text == "On") ? "on" : "off";
            var out3 = (Out3.Text == "On") ? "on" : "off";
            var out4 = (Out4.Text == "On") ? "on" : "off";

            var val = 0;
            if (out1 == "on")
            {
                val += 1;
            };
            if (out2 == "on")
            {
                val += 2;
            };
            if (out3 == "on")
            {
                val += 4;
            };
            if (out4 == "on")
            {
                val += 8;
            };

            Data data = new Data();
            data.ip = devIpAddr.Text;
            data.devNum = devNum.Text;
            data.data = val;

            sendData(data);
        }

        void sendData(Data data)
        {
            var host = hostIpAddr.Text;            
            var uriString = string.Format("http://{0}:3000/ismaView/control/setOutputs?ip={1}&devNum={2}&data={3}", host, data.ip,data.devNum, data.data);
            Uri uri = new Uri(uriString);            
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);            
            req.Method = "GET";
            try
            {
               req.BeginGetResponse(setResponseHandler, req);               
            }
            catch(Exception e)
            {
                Debug.WriteLine("e.send: " +e);                
            }         
        }

        async void setResponseHandler(IAsyncResult res)
        {
            var req = res.AsyncState as HttpWebRequest;
            try
            {
                var response = req.EndGetResponse(res);
                isReady = true;
                Device.BeginInvokeOnMainThread(() =>
                {
                    btnWrite.BackgroundColor = Color.Black;
                });
                await Task.Delay(50);
            }
            catch (Exception e)
            {
                Debug.WriteLine("e.setresp: " +e);
            }          
        }

        async void getResponseHandler(IAsyncResult res)
        {
            var req =  res.AsyncState as HttpWebRequest;
            try
            {
                 var response = req.EndGetResponse(res);                
                 RenderStream(response.GetResponseStream());
                 isReady = true;
                await Task.Delay(50);
            }
            catch (Exception e)
            {                
                Debug.WriteLine("e.getresp : " +e);
            }
        }

        public void RenderStream(Stream stream)
        {
            var reader = new System.IO.StreamReader(stream);
            string res = reader.ReadToEnd();
            Inputs ins = JsonConvert.DeserializeObject<Inputs>(res);
            
            Device.BeginInvokeOnMainThread(() => {
                Input1.Text = (ins.in1 == "1") ? "On" : "Off";
            });
            Device.BeginInvokeOnMainThread(() => {
                Input2.Text = (ins.in2 == "1") ? "On" : "Off";
            });
            Device.BeginInvokeOnMainThread(() => {
                Input3.Text = (ins.in3 == "1") ? "On" : "Off";
            });
            Device.BeginInvokeOnMainThread(() => {
                Input4.Text = (ins.in4 == "1") ? "On" : "Off";
            });

            Device.BeginInvokeOnMainThread(() => {
                Input1.BackgroundColor = (Input1.Text == "On") ? Color.Green : Color.Red;
            });
            Device.BeginInvokeOnMainThread(() => {
                Input2.BackgroundColor = (Input2.Text == "On") ? Color.Green : Color.Red;
            });
            Device.BeginInvokeOnMainThread(() => {
                Input3.BackgroundColor = (Input3.Text == "On") ? Color.Green : Color.Red;
            });
            Device.BeginInvokeOnMainThread(() => {
                Input4.BackgroundColor = (Input4.Text == "On") ? Color.Green : Color.Red;
            });
            Device.BeginInvokeOnMainThread(() => {
                btnRead.BackgroundColor = Color.Black;
            });
        }
        
        public class Inputs
        {
            public string in1 { get; set; }
            public string in2 { get; set; }
            public string in3 { get; set; }
            public string in4 { get; set; }
        }

        
        async public void LiveConnect(object sender, EventArgs e)
        {
            isLive = !isLive;
            if (isLive)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    btnLive.BackgroundColor = Color.Green;
                });
                while (isLive)
                {                    
                    if (isReady) { readData(); }                    
                    await Task.Delay(250);
                    if (isReady) { setOutputs(); }
                    await Task.Delay(250);                    
                }
            }
            else
            {
                btnLive.BackgroundColor = Color.Black;
            }
        }
    }
}
