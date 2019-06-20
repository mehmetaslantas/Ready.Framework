//namespace Ready.Framework.Extensions
//{
//    public static class HttpRequestExtension
//    {
//        private static string GetUserHostAddress(this HttpRequest request)
//        {
//            var hostAddress = string.Empty;
//            if (HttpContext.Current != null)
//            {
//                hostAddress = request.ServerVariables.AllKeys.Contains("HTTP_X_FORWARDED_FOR") && !string.IsNullOrEmpty(request.ServerVariables["HTTP_X_FORWARDED_FOR"])
//                    ? request.ServerVariables["HTTP_X_FORWARDED_FOR"]
//                    : request.ServerVariables["REMOTE_ADDR"];
//                var ipAddresses = hostAddress.Split(',');
//                if (ipAddresses.Length > 1)
//                {
//                    for (var i = 0; i <= ipAddresses.Length - 1; i++)
//                        if (!ipAddresses[i].StartsWith("10.") && !ipAddresses[i].StartsWith("192.168."))
//                        {
//                            var subnets = ipAddresses[i].Split('.');
//                            var nSubnet = 0;
//                            int.TryParse(subnets[1], out nSubnet);
//                            if (subnets[0] != "172" || nSubnet < 16 || nSubnet > 31)
//                            {
//                                hostAddress = ipAddresses[i];
//                                break;
//                            }
//                        }
//                    if (string.IsNullOrEmpty(hostAddress))
//                        hostAddress = ipAddresses[0];
//                }
//                if (hostAddress.StartsWith("127.0.0") || hostAddress.StartsWith("192.168") || hostAddress.StartsWith("::1"))
//                    hostAddress = "212.252.206.35";
//            }
//            return hostAddress;
//        }

//        public static string GetClientIpAddress(this HttpRequest request)
//        {
//            if (request == null)
//                return null;

//            try
//            {
//                if (ReferenceEquals(HttpContext.Current, null))
//                    return null;

//                var customIpAddress = HttpContext.Current.Request.Headers["X-ClientIp"];
//                var ipAddress = customIpAddress ?? HttpContext.Current.Request.GetUserHostAddress();

//                return ipAddress?.Split(':').FirstOrDefault();
//            }
//            catch
//            {
//                //silent
//            }
//            return null;
//        }
//    }
//}