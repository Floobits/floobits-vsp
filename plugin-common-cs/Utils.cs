﻿using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Floobits.Utilities;

namespace Floobits.Common
{
    public class Utils {
#if LATER
        public static String stackToString(Exception e){
            StringWriter sw = new StringWriter();
            PrintWriter pw = new PrintWriter(sw);
            e.printStackTrace(pw);
            return sw.toString(); // stack trace as a string
        }
#endif
        public static string absPath (string p1, string path) {
            return FilenameUtils.concat(p1, path);
        }

        public static bool isShared (string path, string p1) {
            return isChild(path, p1);
        }

        public static string toProjectRelPath (string path, string p1) {
            try {
                return getRelativePath(path, p1);
            } catch (PathResolutionException) {
                return null;
            }
        }

        private static readonly string cert =
            "-----BEGIN CERTIFICATE-----\n" +
            "MIIHyTCCBbGgAwIBAgIBATANBgkqhkiG9w0BAQUFADB9MQswCQYDVQQGEwJJTDEW\n" +
            "MBQGA1UEChMNU3RhcnRDb20gTHRkLjErMCkGA1UECxMiU2VjdXJlIERpZ2l0YWwg\n" +
            "Q2VydGlmaWNhdGUgU2lnbmluZzEpMCcGA1UEAxMgU3RhcnRDb20gQ2VydGlmaWNh\n" +
            "dGlvbiBBdXRob3JpdHkwHhcNMDYwOTE3MTk0NjM2WhcNMzYwOTE3MTk0NjM2WjB9\n" +
            "MQswCQYDVQQGEwJJTDEWMBQGA1UEChMNU3RhcnRDb20gTHRkLjErMCkGA1UECxMi\n" +
            "U2VjdXJlIERpZ2l0YWwgQ2VydGlmaWNhdGUgU2lnbmluZzEpMCcGA1UEAxMgU3Rh\n" +
            "cnRDb20gQ2VydGlmaWNhdGlvbiBBdXRob3JpdHkwggIiMA0GCSqGSIb3DQEBAQUA\n" +
            "A4ICDwAwggIKAoICAQDBiNsJvGxGfHiflXu1M5DycmLWwTYgIiRezul38kMKogZk\n" +
            "pMyONvg45iPwbm2xPN1yo4UcodM9tDMr0y+v/uqwQVlntsQGfQqedIXWeUyAN3rf\n" +
            "OQVSWff0G0ZDpNKFhdLDcfN1YjS6LIp/Ho/u7TTQEceWzVI9ujPW3U3eCztKS5/C\n" +
            "Ji/6tRYccjV3yjxd5srhJosaNnZcAdt0FCX+7bWgiA/deMotHweXMAEtcnn6RtYT\n" +
            "Kqi5pquDSR3l8u/d5AGOGAqPY1MWhWKpDhk6zLVmpsJrdAfkK+F2PrRt2PZE4XNi\n" +
            "HzvEvqBTViVsUQn3qqvKv3b9bZvzndu/PWa8DFaqr5hIlTpL36dYUNk4dalb6kMM\n" +
            "Av+Z6+hsTXBbKWWc3apdzK8BMewM69KN6Oqce+Zu9ydmDBpI125C4z/eIT574Q1w\n" +
            "+2OqqGwaVLRcJXrJosmLFqa7LH4XXgVNWG4SHQHuEhANxjJ/GP/89PrNbpHoNkm+\n" +
            "Gkhpi8KWTRoSsmkXwQqQ1vp5Iki/untp+HDH+no32NgN0nZPV/+Qt+OR0t3vwmC3\n" +
            "Zzrd/qqc8NSLf3Iizsafl7b4r4qgEKjZ+xjGtrVcUjyJthkqcwEKDwOzEmDyei+B\n" +
            "26Nu/yYwl/WL3YlXtq09s68rxbd2AvCl1iuahhQqcvbjM4xdCUsT37uMdBNSSwID\n" +
            "AQABo4ICUjCCAk4wDAYDVR0TBAUwAwEB/zALBgNVHQ8EBAMCAa4wHQYDVR0OBBYE\n" +
            "FE4L7xqkQFulF2mHMMo0aEPQQa7yMGQGA1UdHwRdMFswLKAqoCiGJmh0dHA6Ly9j\n" +
            "ZXJ0LnN0YXJ0Y29tLm9yZy9zZnNjYS1jcmwuY3JsMCugKaAnhiVodHRwOi8vY3Js\n" +
            "LnN0YXJ0Y29tLm9yZy9zZnNjYS1jcmwuY3JsMIIBXQYDVR0gBIIBVDCCAVAwggFM\n" +
            "BgsrBgEEAYG1NwEBATCCATswLwYIKwYBBQUHAgEWI2h0dHA6Ly9jZXJ0LnN0YXJ0\n" +
            "Y29tLm9yZy9wb2xpY3kucGRmMDUGCCsGAQUFBwIBFilodHRwOi8vY2VydC5zdGFy\n" +
            "dGNvbS5vcmcvaW50ZXJtZWRpYXRlLnBkZjCB0AYIKwYBBQUHAgIwgcMwJxYgU3Rh\n" +
            "cnQgQ29tbWVyY2lhbCAoU3RhcnRDb20pIEx0ZC4wAwIBARqBl0xpbWl0ZWQgTGlh\n" +
            "YmlsaXR5LCByZWFkIHRoZSBzZWN0aW9uICpMZWdhbCBMaW1pdGF0aW9ucyogb2Yg\n" +
            "dGhlIFN0YXJ0Q29tIENlcnRpZmljYXRpb24gQXV0aG9yaXR5IFBvbGljeSBhdmFp\n" +
            "bGFibGUgYXQgaHR0cDovL2NlcnQuc3RhcnRjb20ub3JnL3BvbGljeS5wZGYwEQYJ\n" +
            "YIZIAYb4QgEBBAQDAgAHMDgGCWCGSAGG+EIBDQQrFilTdGFydENvbSBGcmVlIFNT\n" +
            "TCBDZXJ0aWZpY2F0aW9uIEF1dGhvcml0eTANBgkqhkiG9w0BAQUFAAOCAgEAFmyZ\n" +
            "9GYMNPXQhV59CuzaEE44HF7fpiUFS5Eyweg78T3dRAlbB0mKKctmArexmvclmAk8\n" +
            "jhvh3TaHK0u7aNM5Zj2gJsfyOZEdUauCe37Vzlrk4gNXcGmXCPleWKYK34wGmkUW\n" +
            "FjgKXlf2Ysd6AgXmvB618p70qSmD+LIU424oh0TDkBreOKk8rENNZEXO3SipXPJz\n" +
            "ewT4F+irsfMuXGRuczE6Eri8sxHkfY+BUZo7jYn0TZNmezwD7dOaHZrzZVD1oNB1\n" +
            "ny+v8OqCQ5j4aZyJecRDjkZy42Q2Eq/3JR44iZB3fsNrarnDy0RLrHiQi+fHLB5L\n" +
            "EUTINFInzQpdn4XBidUaePKVEFMy3YCEZnXZtWgo+2EuvoSoOMCZEoalHmdkrQYu\n" +
            "L6lwhceWD3yJZfWOQ1QOq92lgDmUYMA0yZZwLKMS9R9Ie70cfmu3nZD0Ijuu+Pwq\n" +
            "yvqCUqDvr0tVk+vBtfAii6w0TiYiBKGHLHVKt+V9E9e4DGTANtLJL4YSjCMJwRuC\n" +
            "O3NJo2pXh5Tl1njFmUNj403gdy3hZZlyaQQaRwnmDwFWJPsfvw55qVguucQJAX6V\n" +
            "um0ABj6y6koQOdjQK/W/7HW/lwLFCRsI3FU34oH7N4RDYiDK51ZLZer+bMEkkySh\n" +
            "NOsF/5oirpt9P/FlUQqmMGqz9IgcgA38corog14=\n" +
            "-----END CERTIFICATE-----";

        public static bool isSamePath (string p1, string p2) {
            p1 = FilenameUtils.normalizeNoEndSeparator(p1);
            p2 = FilenameUtils.normalizeNoEndSeparator(p2);
            return p1.Equals(p2);
        }

        public static string unFuckPath (string path) {
    	    return FilenameUtils.normalize(Path.GetFullPath(path));
        }

        public static bool isChild (string path, string parent) {
            try {
                string unFuckedPath = unFuckPath(path);
                string relativePath = getRelativePath(unFuckedPath, parent);
                return !relativePath.Contains("..");
            } catch (Exception) {
                return false;
            }
        }

        /** 
         * see http://stackoverflow.com/questions/204784/how-to-construct-a-relative-file-in-java-from-two-absolute-paths-or-urls/3054692#3054692
         * Get the relative file from one file to another, specifying the directory separator.
         * If one of the provided resources does not exist, it is assumed to be a file unless it ends with '/' or
         * '\'.
         * Pretty straight forward C# port
         * 
         * @param targetPath targetPath is calculated to this file
         * @param basePath basePath is calculated from this file
         * @return String
         */
        public static string getRelativePath (string targetPath, string basePath)
        {
            if (targetPath == null || basePath == null) {
                return null;
            }
            // Normalize the paths
            string pathSeparator = Path.PathSeparator.ToString();
            string normalizedBasePath = FilenameUtils.normalizeNoEndSeparator(basePath);
            string normalizedTargetPath = FilenameUtils.normalizeNoEndSeparator(targetPath);

            // Undo the changes to the separators made by normalization
            if (pathSeparator.Equals("/")) {
                normalizedTargetPath = FilenameUtils.separatorsToUnix(normalizedTargetPath);
                normalizedBasePath = FilenameUtils.separatorsToUnix(normalizedBasePath);

            } else if (pathSeparator.Equals("\\")) {
                normalizedTargetPath = FilenameUtils.separatorsToWindows(normalizedTargetPath);
                normalizedBasePath = FilenameUtils.separatorsToWindows(normalizedBasePath);
            } else {
                throw new ArgumentException("Unrecognised dir separator '" + pathSeparator + "'");
            }

            string[] basepath = Regex.Split(normalizedBasePath, pathSeparator);
            string[] targetpath = Regex.Split(normalizedTargetPath, pathSeparator);

            // First get all the common elements. Store them as a string,
            // and also count how many of them there are.
            StringBuilder common = new StringBuilder();

            int commonIndex = 0;
            while (commonIndex < targetpath.Length && commonIndex < basepath.Length
                    && targetpath[commonIndex].Equals(basepath[commonIndex])) {
                common.Append(targetpath[commonIndex]).Append(pathSeparator);
                commonIndex++;
            }

            if (commonIndex == 0) {
                // No single common file element. This most
                // likely indicates differing drive letters, like C: and D:.
                // These paths cannot be relativized.
                throw new PathResolutionException("No common file element found for '" + normalizedTargetPath + "' and '" + normalizedBasePath
                        + "'");
            } 

            // The number of directories we have to backtrack depends on whether the base is a file or a dir
            // For example, the relative file from
            //
            // /foo/bar/baz/gg/ff to /foo/bar/baz
            // 
            // ".." if ff is a file
            // "../.." if ff is a directory
            //
            // The following is a heuristic to figure out if the base refers to a file or dir. It's not perfect, because
            // the resource referred to by this file may not actually exist, but it's the best I can do
            bool baseIsFile = true;

            string baseResource = string.Copy(normalizedBasePath);

            if (File.Exists(baseResource)) {
                baseIsFile = (File.GetAttributes(baseResource) & FileAttributes.Directory) != FileAttributes.Directory;
            } else if (basePath.EndsWith(pathSeparator)) {
                baseIsFile = false;
            }

            StringBuilder relative = new StringBuilder();

            if (basepath.Length != commonIndex) {
                int numDirsUp = baseIsFile ? basepath.Length - commonIndex - 1 : basepath.Length - commonIndex;

                for (int i = 0; i < numDirsUp; i++) {
                    relative.Append("..").Append(pathSeparator);
                }
            }
            String commonStr = common.ToString();
            // Handle missing trailing slash issues with base project directory:
            if (normalizedTargetPath.Equals(commonStr.Substring(0, commonStr.Length - 1))) {
                return "";
            }
            relative.Append(normalizedTargetPath.Substring(commonStr.Length));
            return relative.ToString();
        }

        class PathResolutionException : ApplicationException {
            public PathResolutionException (string msg) : base(msg) {}
        }

#if LATER
        static public SSLContext createSSLContext() {
            X509TrustManager x509TrustManager = new X509TrustManager() {
                public X509Certificate[] getAcceptedIssuers() {return null;}
                public void checkClientTrusted(X509Certificate[] certs, String authType) {}
                public void checkServerTrusted(X509Certificate[] certs, String authType) throws CertificateException {
                    InputStream is = new ByteArrayInputStream(cert.getBytes());
                    CertificateFactory cf;
                    X509Certificate cert;

                    cf = CertificateFactory.getInstance("X.509");
                    cert = (X509Certificate) cf.generateCertificate(is);
                    cert.checkValidity();

                    ArrayList<X509Certificate> x509Certificates = new ArrayList<X509Certificate>(Arrays.asList(certs));
                    x509Certificates.add(cert);
                    X509Certificate a, b;
                    a = x509Certificates.get(0);
                    for (int i = 1; i < x509Certificates.size(); i++) {
                        b = x509Certificates.get(i);
                        a.checkValidity();
                        try {
                            a.verify(b.getPublicKey());
                        } catch (Exception e) {
                            Flog.warn(e);
                            throw new CertificateException(e);
                        }
                        a = b;
                    }
                }
            };

            SSLContext sc = null;
            try {
                sc = SSLContext.getInstance("SSL");
                sc.init(null, new javax.net.ssl.TrustManager[]{x509TrustManager}, new SecureRandom());
            } catch (Exception e) {
                Flog.warn(e);
            }
            return sc;
        }
#endif
    }


    // Porting Simplicity
    public static partial class JavaExtensions
    {
        public static string JavaSubstring(this string s, int start, int end)
        {
            return s.Substring(start, end - start);
        }
    }
}
