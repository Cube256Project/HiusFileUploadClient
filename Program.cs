using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;

namespace HiusFileUploadClient
{
    /// <summary>
    /// P2824-12: Beispielprogram für Automatisierte Bestellungen.
    /// </summary>
    /// <remarks>
    /// <para>
    /// See <x href="https://shop.hius.ch/manuals" >HIUS Online Manuals</x>.
    /// </para>
    /// </remarks>
    class Program
    {
        #region Parameters

        public string City = "Teststadt";

        public int CustomerID = 0;

        public string PostalCode = "9999";

        public int Sequence = 0;

        public string ServicePassword = "kM9SgP6aTMe6CSva";

        // public string ServiceURI = "https://shop.hius.ch/upload/";
        public string ServiceURI = "http://shop.hius.local:2222/upload/";

        public string UserName = "u2960@hius.ch";

        public string VerwaltungName = "Beispiel AG";

        public string FacilityManagerName = "Beispiel Facility GmbH";

        #endregion

        #region Entry Point

        /// <summary>
        /// Program entry-point.
        /// </summary>
        /// <param name="args">Unused.</param>
        public static void Main(string[] args)
        {
            try
            {
                new Program().Run();
            }
            catch (Exception ex)
            {
                Trace("## general error: " + ex.Message);
            }
        }

        #endregion

        #region Diagnostics

        /// <summary>
        /// Diagnostic output.
        /// </summary>
        /// <param name="message"></param>
        private static void Trace(string message) => Console.WriteLine(message);

        #endregion

        #region Private Methods

        /// <summary>
        /// Main procedure.
        /// </summary>
        private void Run()
        {
            HttpWebRequest request;
            HttpWebResponse response;
            Stream input, output;
            string filename;

            filename = "HLV2824_Q_" + CustomerID + "_" + Sequence + ".csv";

            // prepare request
            request = (HttpWebRequest)WebRequest.Create(new Uri(new Uri(ServiceURI), filename));
            request.Method = "PUT";
            request.ContentType = "text/csv";

            // always send credential
            request.PreAuthenticate = true;
            request.Credentials = new NetworkCredential(UserName, ServicePassword);

            // needed for proxy-based access (default port)
            request.CookieContainer = new CookieContainer();

            // request payload
            input = GenerateRequestFile();

            // request content
            input.CopyTo(request.GetRequestStream());

            // diagnostics
            input.Position = 0;
            Trace("-- request:\n" + new StreamReader(input).ReadToEnd());

            try
            {
                // get response
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                response = ex.Response as HttpWebResponse;

                if (null == response)
                {
                    throw;
                }
            }

            // capture response content
            output = new MemoryStream();
                response.GetResponseStream().CopyTo(output);
                output.Position = 0;

            // diagnostics
            Trace("-- response " + (int)response.StatusCode 
                + " " + response.ContentType + ":"
                + "\n" + new StreamReader(output).ReadToEnd());
        }

        /// <summary>
        /// Generates a minimal but complete request file.
        /// </summary>
        /// <returns>
        /// The encoded request.
        /// </returns>
        private Stream GenerateRequestFile()
        {
            MemoryStream stream;
            StreamWriter writer;

            stream = new MemoryStream();
            using (writer = new StreamWriter(stream, Encoding.UTF8, 0x1000, true))
            {
                // 1: Adressen
                // Es werden kundeneigene Bezeichner verwendet.
                // x:1: Adresse der Verwaltung
                writer.WriteLine(string.Join(';', new object[]
                {
                    "1",
                    "request1",
                    "VW",
                    "x:1",
                    VerwaltungName,
                    "Bahnhofplatz 1",
                    PostalCode,
                    City
                }));

                // x:2: Adresse der Hauswartung
                writer.WriteLine(string.Join(';', new object[] 
                { 
                    "1",
                    "request2",
                    "HW",
                    "x:2",
                    FacilityManagerName, 
                    "Vorhof 2", 
                    PostalCode, 
                    City
                }));

                // x:3: Adresse einer Liegenschaft
                writer.WriteLine(string.Join(';', new object[]
                {
                    "1",
                    "request3",
                    null,
                    "x:3",
                    "Villa am See",
                    "Seitenweg 3",
                    PostalCode,
                    City
                }));

                // 2: Liegenschaften Profile
                // Siehe HIUS Produktkatalog
                writer.WriteLine(string.Join(";", new object[]
                    {
                        "2",
                        "request4",
                        "U",
                        "x:3",
                        null,
                        "01.200.46/BK,h=20mm,w=100mm",
                        "01.200.46/SU,h=20mm,w=100mm"
                    })); ;

                // 3: Bestellung
                writer.WriteLine(string.Join(";", new object[] 
                {
                    "3",
                    "request5",
                    "VB",
                    "x:3",
                    null,
                    "2020-02-20",
                    "Bernasconi",
                    "Maria"
                }));
            }

            stream.Position = 0;
            return stream;
        }

        #endregion
    }
}
