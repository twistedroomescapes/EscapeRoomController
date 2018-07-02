using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;


namespace EscapeRoomController
{


    public class RemotingSettings
    {
        public string MachineName { get; set; }
        public string RemotingURI { get; set; }
        public int RemotingPort { get; set; }
        public string Service { get; set; }


        public RemotingSettings()
        { 
        }


        public RemotingSettings(string name)
        {
            MachineName = ConfigurationManager.AppSettings[string.Format("{0}MachineName", name)];
            RemotingURI = ConfigurationManager.AppSettings[string.Format("{0}RemotingURI", name)];
            RemotingPort = int.Parse(ConfigurationManager.AppSettings[string.Format("{0}RemotingPort", name)]);
            Service = ConfigurationManager.AppSettings[string.Format("{0}Service", name)];
        }

    }

    public class RemotingHelper
    {

        public static string StartService(string serviceName, int timeoutMilliseconds = 10000)
        {
            string msg = string.Empty;

            ServiceController service = new ServiceController(serviceName);
            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                msg = GetServiceStatus(serviceName);
            }
            catch (Exception e)
            {
                msg = e.ToString();
            }

            return msg;
        }


        public static string StopService(string serviceName, int timeoutMilliseconds = 10000)
        {
            string msg = string.Empty;

            ServiceController service = new ServiceController(serviceName);
            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                msg = GetServiceStatus(serviceName);
            }
            catch (Exception e)
            {
                msg = e.ToString();
            }

            return msg;
        }


        public static bool IsServiceRunning(string serviceName)
        {
            var service = new ServiceController(serviceName);

            if ((service.Status.Equals(ServiceControllerStatus.Stopped)) || (service.Status.Equals(ServiceControllerStatus.StopPending)))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static string GetServiceStatus(string serviceName)
        {
            var service = new ServiceController(serviceName);

            return service.Status.ToString();
        }


 

    }

    [Serializable]
    public class MyMessage
    {
        public string TimeRemaining { get; set; }
        public string Document { get; set; }
    }


    public sealed class RemoteListener : System.MarshalByRefObject, IRemoteListener
    {

        public RemoteListener()
        {
        }

        public string GetStatus()
        {
            return "Ok";
        }

        public MyMessage GetAllData()
        {
            return EscapeRoomController.MyMessage;
        }

    }

    public interface IRemoteListener
    {
        string GetStatus();
        MyMessage GetAllData();
    }
}
