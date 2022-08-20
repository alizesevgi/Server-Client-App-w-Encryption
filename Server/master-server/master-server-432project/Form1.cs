using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Security.Cryptography.RNGCryptoServiceProvider;


namespace master_server_432project
{
    public partial class Form1 : Form
    {
        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        List<Socket> clientSockets = new List<Socket>();
  
        private static Mutex mutex = new Mutex();
        Socket server1;
        Socket server2;
        bool server1conn = false;
        bool server2conn = false; 
        bool terminating = false;
        bool listening = false;
        bool serversConnected = false;

        string xml3072_bit_key_Master = "";
        string string_dec_file = "";
        string string_dec_filename = "";
        string dec_filename = "";
        string dec_file = "";
        string fileContent = string.Empty;
        string rsaofserver;
        //List<string> server1queue = new List<string>();
        //List<string> server2queue = new List<string>();
        List<string> filenamesqueue = new List<string>();
        List<string> senttoserver2 = new List<string>();
        List<string> senttoserver1 = new List<string>();
        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e) //Listen button
        {
            int serverPort;
            
            if (Int32.TryParse(port_num_text.Text, out serverPort))
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, serverPort);
                serverSocket.Bind(endPoint);
                serverSocket.Listen(3);

                listening = true;
                button1.Enabled = false;

                //button_send.Enabled = true;

                Thread acceptThread = new Thread(Accept);
                acceptThread.Start();

                richTextBox1.AppendText("Started listening on port: " + serverPort + "\n");

              
            }
            else
            {
                richTextBox1.AppendText("Please check port number \n");
            }

        }
        void CheckDupes()
        {
            List<string> server1dir = new List<string>();
            if (Directory.Exists(@"c:\Server1\Files\"))
            {
                if (Directory.GetFiles(@"c:\Server1\Files\").Length != 0)
                {

                    DirectoryInfo dir1 = new DirectoryInfo(@"c:\Server1\Files\");
                    FileInfo[] files1 = dir1.GetFiles();
                    foreach (FileInfo file in files1)
                    {
                        server1dir.Add(file.Name);
                    }
                }

            }
            List<string> server2dir = new List<string>();
            if (Directory.Exists(@"c:\Server2\Files\"))
            {
                if (Directory.GetFiles(@"c:\Server2\Files\").Length != 0)
                {

                    DirectoryInfo dir2 = new DirectoryInfo(@"c:\Server2\Files\");
                    FileInfo[] files2 = dir2.GetFiles();
                    foreach (FileInfo file in files2)
                    {
                        server2dir.Add(file.Name);
                    }
                }

            }
            if (Directory.Exists(@"c:\MasterServer\Files\"))
            {
                if (Directory.GetFiles(@"c:\MasterServer\Files\").Length != 0)
                {

                    List<string> masterdir = new List<string>();
                    DirectoryInfo dirx = new DirectoryInfo(@"c:\MasterServer\Files\");
                    if (Directory.GetFiles(@"c:\MasterServer\Files\").Length != 0)
                    {
                        FileInfo[] filesx = dirx.GetFiles();
                        foreach (FileInfo file in filesx)
                        {
                            masterdir.Add(file.Name);
                        }

                        List<string> templist = new List<string>();
                        templist = masterdir.Except(server1dir).ToList();
                        templist.AddRange(masterdir.Except(server2dir).ToList());
                        if (!filenamesqueue.Any())
                        {
                            filenamesqueue = templist.Distinct().ToList();//if empty, do this
                        }
                        else
                        {
                            filenamesqueue.AddRange(filenamesqueue.Except(templist.Distinct()).ToList());//if not empty, do this
                        }
                        SendQueue();
                    }
                }

            }

        }
        private void Accept()
        {
            while (listening)
            {
                try
                {
                    Socket newClient = serverSocket.Accept();
                    clientSockets.Add(newClient);
                    richTextBox1.AppendText("Someone is trying to connect. \n");

                    Thread checkThread = new Thread(() => Check(newClient));
                    checkThread.Start();
                }
                catch
                {
                    if (terminating)
                    {
                        listening = false;
                    }
                    else
                    {
                        richTextBox1.AppendText("Socket has stopped working. \n");
                    }
                }
            }
        }

        byte[] byteArray = new byte[48];
        byte[] myAESkey = new byte[16];
        byte[] myIV = new byte[16];
        byte[] myHMACkey = new byte[16];
        string RSAPublicKey3072_Server1;
        string RSAPublicKey3072_Server2;
        private void Check(Socket thisClient) // updated
        {
            bool trying = true;
            string server_id = "x";
            while (trying && !terminating)
            {
                try
                {
                    Byte[] buffer = new Byte[128];
                    thisClient.Receive(buffer);

                    server_id = Encoding.Default.GetString(buffer);
                    
                    server_id = server_id.Substring(0, server_id.IndexOf("\0"));

                    if(server_id.Length>10)
                    {
                        server_id = server_id.Substring(0, 8);
                        serversConnected = true;

                        //Thread dummyThread = new Thread(() => sendKey()); // updated
                        //dummyThread.Start();


                    }
                  
                    
                    richTextBox1.AppendText("A user entered their role as; " + server_id + "\n");

                    richTextBox1.AppendText(server_id + " has successfully connected.\n");

                    if (server_id == "client")
                    {
                        Byte[] buffer2 = Encoding.Default.GetBytes("You have connected to the MASTER");
                        thisClient.Send(buffer2);
                        //client knows which server it connects
                        Thread receiveThread = new Thread(() => Receive(thisClient, server_id)); // updated
                        receiveThread.Start();

                    }
                    else
                    {


                        //the session key info ( random byte array with 48 bytes) is encrypted using the RSA-3072 public key of the requested server 
                        //and signed with RSA-3072 private key of the master server
                    

                        //Server 1 Pub

                        using (System.IO.StreamReader fileReader =
                        new System.IO.StreamReader("Server1_pub.txt"))
                        {
                            RSAPublicKey3072_Server1 = fileReader.ReadLine();

                        }

                        //Server 2 Pub

                        using (System.IO.StreamReader fileReader =
                        new System.IO.StreamReader("Server2_pub.txt"))
                        {
                            RSAPublicKey3072_Server2 = fileReader.ReadLine();
                        }

                        //Master Key

                        using (System.IO.StreamReader fileReader =
                        new System.IO.StreamReader("MasterServer_pub_prv.txt"))
                        {
                            xml3072_bit_key_Master = fileReader.ReadLine();
                        }


                        if (server_id == "Server 1")
                        {

          
                            Byte[] buffer4 = Encoding.Default.GetBytes("Hello " + server_id + ", you have successfully connected to the server. \n");
                            thisClient.Send(buffer4);

                            server1 = thisClient;
                            server1conn = true;

                        }
                        else if (server_id == "Server 2")
                        {
                        
                            Byte[] buffer4 = Encoding.Default.GetBytes("Hello " + server_id + ", you have successfully connected to the server. \n");
                            thisClient.Send(buffer4);

                            server2 = thisClient;
                            server2conn = true;
                           

                        }
                        if (server2conn && server1conn )
                        {
                            
                            Thread receive1Thread = new Thread(() => Receive(server1, "Server 1")); // updated
                            receive1Thread.Start();

                            Thread receive2Thread = new Thread(() => Receive(server2, "Server 2")); // updated
                            receive2Thread.Start();

                        }

                       
                    }
                    trying = false;

                }
                catch
                {
                    if (!terminating)
                    {
                        richTextBox1.AppendText(server_id + ":something went wrong while establishing the connection");
                    }
                    thisClient.Close();
                    if(server_id == "Server 1")
                    {
                        server1conn = false;
                    }
                    else if(server_id == "Server 2")
                    {
                        server2conn = false;
                    }
                    trying = false;
                }
            }
        }

        void sendKey()
        {
            if (server1conn == true && server2conn == true)
            {
                using (System.IO.StreamReader fileReader =
                new System.IO.StreamReader("Server1_pub.txt"))
                {
                    RSAPublicKey3072_Server1 = fileReader.ReadLine();

                }

                //Server 2 Pub

                using (System.IO.StreamReader fileReader =
                new System.IO.StreamReader("Server2_pub.txt"))
                {
                    RSAPublicKey3072_Server2 = fileReader.ReadLine();
                }

                //Master Key

                using (System.IO.StreamReader fileReader =
                new System.IO.StreamReader("MasterServer_pub_prv.txt"))
                {
                    xml3072_bit_key_Master = fileReader.ReadLine();
                }

                RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();

                provider.GetBytes(byteArray);
                string session_key = Encoding.Default.GetString(byteArray);

                Array.Copy(byteArray, myAESkey, 16);
                Array.Copy(byteArray, 16, myIV, 0, 16);
                Array.Copy(byteArray, 32, myHMACkey, 0, 16);

                //richTextBox1.AppendText("Session key: " + session_key + " \n");

                byte[] encryptedRSA = encryptWithRSA(session_key, 3072, RSAPublicKey3072_Server1);
                byte[] signed = signWithRSA(session_key, 3072, xml3072_bit_key_Master);
                richTextBox1.AppendText("Encrypted Session Key: " + generateHexStringFromByteArray(encryptedRSA) + " \n");
                //richTextBox1.AppendText("enc + signed session key: " + generateHexStringFromByteArray(signed) + " \n");

                string msg = "hiddenKey" + generateHexStringFromByteArray(encryptedRSA) + "P" + generateHexStringFromByteArray(signed);
                Byte[] buffer_6 = Encoding.Default.GetBytes(msg);
                server1.Send(buffer_6);

                richTextBox1.AppendText("Server 1 has received its session key. \n");

                //Thread receive1Thread = new Thread(() => Receive(thisClient, "Server 1")); // updated
                //receive1Thread.Start();

                byte[] encryptedRSA2 = encryptWithRSA(session_key, 3072, RSAPublicKey3072_Server2); ;
                string message2 = "hiddenKey" + generateHexStringFromByteArray(encryptedRSA2) + "P" + generateHexStringFromByteArray(signed);

                Byte[] buffer_7 = Encoding.Default.GetBytes(message2);
                server2.Send(buffer_7);

                richTextBox1.AppendText("Server 2 has received its session key. \n");

                check = false;
                CheckDupes();
            }


        }

        bool check = true;
        private void Receive(Socket thisClient, string server_id)
        {

            bool connected = true;

            while (!terminating && connected)
            {
                try
                {
                    Byte[] buffer = new Byte[2048576];
                    thisClient.Receive(buffer);

                    string message = Encoding.Default.GetString(buffer);
                    message = message.Substring(0, message.IndexOf("\0"));
                    //richTextBox1.AppendText(message + "\n");
                    if (message.Length > 16 && message.Substring(0, 17) == "Serverisconnected")
                    {
                        serversConnected = true;
                        richTextBox1.AppendText("Servers are connected. Can send replicated files.\n");
                        //richTextBox1.AppendText("1:" + server1conn + "2:" + server2conn + "both:" + serversConnected);

                        if (server2conn && server1conn && serversConnected && check)
                        {
                            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();

                            provider.GetBytes(byteArray);
                            string session_key = Encoding.Default.GetString(byteArray);

                            Array.Copy(byteArray, myAESkey, 16);
                            Array.Copy(byteArray, 16, myIV, 0, 16);
                            Array.Copy(byteArray, 32, myHMACkey, 0, 16);

                            //richTextBox1.AppendText("Session key: " + session_key + " \n");

                            byte[] encryptedRSA = encryptWithRSA(session_key, 3072, RSAPublicKey3072_Server1);
                            byte[] signed = signWithRSA(session_key, 3072, xml3072_bit_key_Master);
                            richTextBox1.AppendText("Encrypted Session Key for Server 1: " + generateHexStringFromByteArray(encryptedRSA) + " \n");
                            //richTextBox1.AppendText("enc + signed session key: " + generateHexStringFromByteArray(signed) + " \n");

                            string msg = "hiddenKey" + generateHexStringFromByteArray(encryptedRSA) + "P" + generateHexStringFromByteArray(signed);
                            Byte[] buffer_6 = Encoding.Default.GetBytes(msg);
                            server1.Send(buffer_6);

                            richTextBox1.AppendText("Server 1 has received its session key. \n");

                            //Thread receive1Thread = new Thread(() => Receive(thisClient, "Server 1")); // updated
                            //receive1Thread.Start();

                            byte[] encryptedRSA2 = encryptWithRSA(session_key, 3072, RSAPublicKey3072_Server2); ;
                            string message2 = "hiddenKey" + generateHexStringFromByteArray(encryptedRSA2) + "P" + generateHexStringFromByteArray(signed);
                            richTextBox1.AppendText("Encrypted Session Key for Server 2: " + generateHexStringFromByteArray(encryptedRSA2) + " \n");

                            Byte[] buffer_7 = Encoding.Default.GetBytes(message2);
                            server2.Send(buffer_7);

                            richTextBox1.AppendText("Server 2 has received its session key. \n");

                            check = false;
                            CheckDupes();
                            //Thread receive2Thread = new Thread(() => Receive(thisClient, "Server 2")); // updated
                            //receive2Thread.Start();

                        }
                        //Thread sendqueueThread = new Thread(() => SendQueue());
                        //sendqueueThread.Start();
                        SendQueue();

                    }
                    else if (message.Length > 27 && message.Substring(0, 27) == "Server 2 failed decryption.")
                    {
                        richTextBox1.AppendText("Server 2 failed decryption.");
                        int startPos = message.LastIndexOf("Server 2 failed decryption.") + 27;
                        int range = message.Length - startPos - 1;
                        string filenameofverif = message.Substring(startPos, range);
                        if (!filenamesqueue.Contains(filenameofverif))
                        {
                            filenamesqueue.Add(filenameofverif);
                        }
                        
                            
                    }
                    else if (message.Length > 27 && message.Substring(0, 27) == "Server 1 failed decryption.")
                    {
                        richTextBox1.AppendText("Server 1 failed decryption.");
                        int startPos = message.LastIndexOf("Server 1 failed decryption.") + 27;
                        int range = message.Length - startPos - 1;
                        string filenameofverif = message.Substring(startPos, range);
                        if (!filenamesqueue.Contains(filenameofverif))
                        {
                            filenamesqueue.Add(filenameofverif);
                        }
                    }
                    else if (message.Length > 10 && message.Substring(0, 10) == "filecoming")
                    {
                        richTextBox1.AppendText("A client sent me a file \n");
                        Thread verifyFileThread = new Thread(() => verifyFile(thisClient, message));
                        verifyFileThread.Start();
                    }
                    else if (message.Length > 8 && message.Substring(0, 8) == "download")
                    {
                        richTextBox1.AppendText("A client would like to download a file \n");
                        Download_Function(thisClient, message);

                    }
                    else if (message.Length > 13 && message.Substring(0, 13) == "repfilecoming")
                    {
                        richTextBox1.AppendText("A server sent me a file \n");
                        verifyReplicate(thisClient, message);
                        
                    }
                    else if (message.Length > 21 && message.Substring(0, 21) == "verificationSignature")
                    {
                        richTextBox1.AppendText(server_id + " sent me its signature \n");
                        //verifySignature(server_id, message, fileContent);
                        
                        Task verifySignatureThread = new Task(() => verifySignature(server_id, message, fileContent));
                        verifySignatureThread.Start();
                        
                    }
                    else if (message.Length > 9 && message.Substring(0, 9) == "HMACError")
                    {
                        richTextBox1.AppendText("Master sent me HMAC error message... Now verifying signature... \n");
                        verifyHMACError(thisClient, message, server_id);

                    }
                    //else if (message.Length > 16 && message.Substring(0,16) == "filenameofverif:")
                    //{
                    //    richTextBox1.AppendText(message + " \n");
                    //    int startPos = message.LastIndexOf("filenameofverif:") + 16;
                    //    int range = message.Length - startPos - 1;
                    //    string filenameofverif = message.Substring(startPos, range);

                    //    if (server_id == "Server 1")
                    //    {
                    //        senttoserver1.Add(filenameofverif);
                    //        if (senttoserver2.Contains(filenameofverif))
                    //        {
                    //            filenamesqueue.RemoveAll(name => name.Contains(filenameofverif));
                    //            //filenamesqueue.Remove(filenameofverif);
                    //        }
                    //    }
                    //    else if(server_id == "Server 2")
                    //    {
                    //        senttoserver2.Add(filenameofverif);
                    //        if (senttoserver1.Contains(filenameofverif))
                    //        {
                    //            filenamesqueue.RemoveAll(name => name.Contains(filenameofverif));
                    //            //filenamesqueue.Remove(filenameofverif);
                    //        }
                    //    }
                    //}

                    else if (message.Length > 17 && message.Substring(0, 18) == "Serverdisconnected")
                    {
                        serversConnected = false;
                        check = true;
                        
                        richTextBox1.AppendText("Server 1 and Server 2 are not connected to eachother.\n");
                    }
                    else
                    {
                        richTextBox1.AppendText(message);
                    }


                }
                catch
                {
                    if (!terminating)
                    {
                        richTextBox1.AppendText("A " + server_id + " is disconnected. \n");
                    }

                    thisClient.Close();
                    clientSockets.Remove(thisClient);
                    if (server_id == "Server 1")
                    {
                        server1conn = false;
                    }
                    else if (server_id == "Server 2")
                    {
                        server2conn = false;
                    }
                    connected = false;
                }

                

            }
        }
        void verifyHMACError(Socket s, string message, string server)
        {
            string errorMessage = message.Substring(0, 9);
            string signature = message.Substring(9);

            if (server == "Server 1")
            {
                rsaofserver = RSAPublicKey3072_Server1;
            }
            else if (server == "Server 2")
            {
                rsaofserver = RSAPublicKey3072_Server2;
            }
            bool verificationResult = verifyWithRSA(errorMessage, 3072, rsaofserver, hexStringToByteArray(signature));
            if (verificationResult == true)
            {
                richTextBox1.AppendText("HMAC error messsage is received with valid signature. \n");
            }
            else
            {
                richTextBox1.AppendText("HMAC error message is received with invalid signature \n");

            }
        }
        async void SendQueue()
        {
            if(server1conn && server2conn && serversConnected)
            {
                foreach (string filename in filenamesqueue.ToList())
                {
                    string filepath = @"c:\MasterServer\Files\";
                    string myPath = Path.Combine(filepath, filename);
                    if (File.Exists(myPath))
                    {
                        byte[] content = File.ReadAllBytes(myPath);
                        string tobesent_fileContent = Encoding.Default.GetString(content);
                        richTextBox1.AppendText("File found in queue... sending now... \n");
                        await Task.Delay(2000);

                        SendToServers(tobesent_fileContent, filename);
                        await Task.Delay(2000);
                       

                    }
                }
            }
           

        }
        void Download_Function(Socket s, string message)
        {


            using (System.IO.StreamReader fileReader =
            new System.IO.StreamReader("MasterServer_pub_prv.txt"))
            {
                xml3072_bit_key_Master = fileReader.ReadLine();
            }
            //richTextBox1.AppendText("download request succesful");
            string filerequest = message.Substring(8);
            //richTextBox1.AppendText("\n file name:" + filerequest);
            string filepath = @"c:\MasterServer\Files\";
            string download_path = System.IO.Path.Combine(filepath, filerequest);
            //MessageBox.Show(filepath);

            if (System.IO.File.Exists(download_path))
            {
                richTextBox1.AppendText("Requested file found... sending now... \n");
                byte[] content = File.ReadAllBytes(download_path);
                string download_fileContent = Encoding.Default.GetString(content);
                //richTextBox1.AppendText(download_fileContent);

                byte[] signature = signWithRSA(download_fileContent, 3072, xml3072_bit_key_Master);
                string messageToSent = "ResponseDownload" + generateHexStringFromByteArray(signature) + "P" + generateHexStringFromByteArray(content) + "V" + filerequest;
                richTextBox1.AppendText("Requested file sent \n");

                Byte[] buffer = Encoding.Default.GetBytes(messageToSent);
                s.Send(buffer);

            }
            else
            {
                richTextBox1.AppendText("No such file is found, sending error message to the client \n");
                string errormes = "FileNotFound";
                byte[] errormessage = Encoding.Default.GetBytes(errormes);
                byte[] signature = signWithRSA(errormes, 3072, xml3072_bit_key_Master);
                string messageToSent = "ResponseDownload" + generateHexStringFromByteArray(signature) + "P" + generateHexStringFromByteArray(errormessage) + "V" + filerequest;
                Byte[] buffer = Encoding.Default.GetBytes(messageToSent);
                s.Send(buffer);

            }
        }
        static bool verifyWithRSA(string input, int algoLength, string xmlString, byte[] signature)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);
            // create RSA object from System.Security.Cryptography
            RSACryptoServiceProvider rsaObject = new RSACryptoServiceProvider(algoLength);
            // set RSA object with xml string
            rsaObject.FromXmlString(xmlString);
            bool result = false;

            try
            {
                result = rsaObject.VerifyData(byteInput, "SHA256", signature);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }
        async void verifySignature(string server, string message, string fileContent)
        {
            int startPos = message.LastIndexOf("verificationSignature") + 21;
            int indexK = message.IndexOf("P");
            int length = indexK - startPos;
            string Signature = message.Substring(startPos, length);
            byte[] signature = hexStringToByteArray(Signature);

            if (server == "Server 1")
            {
                rsaofserver = RSAPublicKey3072_Server1;
            }
            else if(server == "Server 2")
            {
                rsaofserver = RSAPublicKey3072_Server2;
            }

            bool verificationResult = verifyWithRSA(fileContent, 3072, rsaofserver, signature);
            //Değiştireceğini varsayarak bunları burda bırakıyorum
            if (message.IndexOf("P") + 1 != message.Length && verificationResult)
            {
                int length2 = message.Length - indexK - 1;
                string filename = message.Substring(indexK + 1, length2);

                if (server == "Server2")
                {
                    senttoserver2.Add(filename);
                }
                else if (server == "Server 1")
                {
                    senttoserver1.Add(filename);
                }
                await Task.Delay(2000);
                if (senttoserver1.Contains(filename) && senttoserver2.Contains(filename))
                {
                    filenamesqueue.RemoveAll(name => name.Contains(filename));

                }

            }
            if (verificationResult == true)
            {
                richTextBox1.AppendText("Valid signature, server received my file \n");
                Byte[] buffer4 = Encoding.Default.GetBytes("Master verified my signature.");
                if (server == "Server 1")
                {                   
                    server1.Send(buffer4);
                }
                else if(server == "Server 2")
                {
                    server2.Send(buffer4);
                }
            }
            else
            {
                richTextBox1.AppendText("Invalid signature \n");
            }


        }
        void verifyFile(Socket s, string message)
        {


            using (System.IO.StreamReader fileReader =
            new System.IO.StreamReader("MasterServer_pub_prv.txt"))
            {
                xml3072_bit_key_Master = fileReader.ReadLine();
            }

            //dummy string s= "filecomingAAAAPBBBVCCTD
            int startPos = message.LastIndexOf("filecoming") + 10;
            int indexP = message.IndexOf("P");
            int length = indexP - startPos;
            string encryptedHEXStringFileContent = message.Substring(startPos, length);

            //richTextBox1.AppendText("File content: " + encryptedHEXStringFileContent + "\n");

            byte[] byteFormat_enc_filecontent = hexStringToByteArray(encryptedHEXStringFileContent);
            string enc_FileContent = Encoding.Default.GetString(byteFormat_enc_filecontent);


            int indexV = message.IndexOf("V");
            int indexT = message.IndexOf("T");

            int length2 = indexV - indexP - 1;
            string fileName = message.Substring(indexP + 1, length2);

            //richTextBox1.AppendText("Filename: " + fileName + "\n");
            byte[] byteFormat_filename = hexStringToByteArray(fileName);
            string enc_FileName = Encoding.Default.GetString(byteFormat_filename);



            int length3 = indexT - indexV - 1;
            string encryptedHEXString_AES_key = message.Substring(indexV + 1, length3);

            //richTextBox1.AppendText("AES Key: " + encryptedHEXString_AES_key + "\n");
            byte[] byteFormat_AES_Key = hexStringToByteArray(encryptedHEXString_AES_key);
            string enc_AES_Key = Encoding.Default.GetString(byteFormat_AES_Key);


            int length4 = message.Length - indexT - 1;
            string encryptedHEXString_iv_key = message.Substring(indexT + 1, length4);

            //richTextBox1.AppendText("IV Key: " + encryptedHEXString_iv_key + "\n");
            byte[] byteFormat_iv_key = hexStringToByteArray(encryptedHEXString_iv_key);
            string enc_iv_Key = Encoding.Default.GetString(byteFormat_iv_key);
            bool check = true;

            try
            {
                byte[] decrypted_AES_Key = decryptWithRSA(enc_AES_Key, 3072, xml3072_bit_key_Master);
                byte[] decrypted_iv = decryptWithRSA(enc_iv_Key, 3072, xml3072_bit_key_Master);
                byte[] decrypted_filename = decryptWithAES128(enc_FileName, decrypted_AES_Key, decrypted_iv);
                byte[] decrypted_file = decryptWithAES128(enc_FileContent, decrypted_AES_Key, decrypted_iv);
                string_dec_file = Encoding.Default.GetString(decrypted_file);
                string_dec_filename = Encoding.Default.GetString(decrypted_filename);
                richTextBox1.AppendText("Decryption complete... Now sending the signature. \n");


            }
            catch
            {
                Byte[] buffer9 = Encoding.Default.GetBytes("Decryption failed. \n");
                s.Send(buffer9);
                check = false;
            }

            if (check)
            {
                //Storee
                //signature, server signs file content with priv key
                //client verifs with public key
                byte[] signature = signWithRSA(string_dec_file, 3072, xml3072_bit_key_Master);
                string messageToSent = "verificationSignature" + generateHexStringFromByteArray(signature) + "P";
                //richTextBox1.AppendText("filecontent: " + string_dec_file);

                Byte[] buffer4 = Encoding.Default.GetBytes(messageToSent);
                s.Send(buffer4);

                //// DOSYA OLUŞTURMA KISMI

                string folderName = @"c:\MasterServer";

                string pathString = System.IO.Path.Combine(folderName, "Files");

                System.IO.Directory.CreateDirectory(pathString);

                pathString = System.IO.Path.Combine(pathString, string_dec_filename);

                byte[] file_content = Encoding.Default.GetBytes(string_dec_file);

                using (System.IO.FileStream fs = System.IO.File.Create(pathString))
                {
                    fs.Write(file_content, 0, file_content.Length);
                }
                if(senttoserver1.Contains(string_dec_filename))
                {
                    int index = senttoserver1.IndexOf(string_dec_file);
                    senttoserver1[index] = string_dec_file + "_old";
                }
                if (senttoserver2.Contains(string_dec_filename))
                {
                    int index = senttoserver2.IndexOf(string_dec_file);
                    senttoserver2[index] = string_dec_file + "_old";
                }

                filenamesqueue.Add(string_dec_filename);
                SendToServers(string_dec_file, string_dec_filename);



            }
            



        }
        public async void SendToServers(string file, string filename)
        {


           
           
            //file and filename aes enc
            byte[] encryptedFileWithAES = encryptWithAES128(file, myAESkey, myIV);
            string encryptedHEXStringFileContent = generateHexStringFromByteArray(encryptedFileWithAES);
            //richTextBox1.AppendText("File content: " + encryptedHEXStringFileContent + "\n");

            byte[] encryptedFileNameWithAES = encryptWithAES128(filename, myAESkey, myIV);
            string encryptedHEXStringFileName = generateHexStringFromByteArray(encryptedFileNameWithAES);
            //richTextBox1.AppendText("Filename: " + encryptedHEXStringFileName + "\n");
            //hmac of file & filename
            byte[] HMACofFile = applyHMACwithSHA256(file, myHMACkey);
            string HMACofFileHEXString = generateHexStringFromByteArray(HMACofFile);
            //richTextBox1.AppendText("HMAC File: " + HMACofFileHEXString + "\n");

            byte[] HMACofFileName = applyHMACwithSHA256(filename, myHMACkey);
            string HMACofFileNameHEXString = generateHexStringFromByteArray(HMACofFileName);
            //richTextBox1.AppendText("HMAC Filename: " + HMACofFileNameHEXString + "\n");

            string file_to_be_sent = "repfilecoming" + encryptedHEXStringFileContent + "P" + encryptedHEXStringFileName + "V" + HMACofFileHEXString + "T" + HMACofFileNameHEXString;
            
            fileContent = file;
            Byte[] buffer6 = Encoding.Default.GetBytes(file_to_be_sent);

            if (server1conn && server2conn && serversConnected && filenamesqueue.Contains(filename))
            {
                richTextBox1.AppendText("Sending a replicated file to servers. \n");
                if (!senttoserver1.Contains(filename))
                {
                    server1.Send(buffer6);
                    richTextBox1.AppendText("Server 1 has been sent replicate file. \n");
                    
                }
                await Task.Delay(2000);
                if (!senttoserver2.Contains(filename))
                {
                    server2.Send(buffer6);
                    richTextBox1.AppendText("Server 2 has been sent replicate file. \n");
                    
                }

                filenamesqueue.Remove(filename);
            }
            else
            {
                richTextBox1.AppendText("File replication cannot occur due to offline servers. File queued. \n");
                //richTextBox1.AppendText("1:"+server1conn+"2:"+server2conn+"both:"+serversConnected);

            }




        }

        void verifyReplicate(Socket s, string message)
        {
            //if the connection has continued and file has been received fully
            //AES128 file
            int startPos = message.LastIndexOf("repfilecoming") + 13;
            int indexP = message.IndexOf("P");
            int length = indexP - startPos;
            string encryptedHEXStringFileContent = message.Substring(startPos, length);

            //richTextBox1.AppendText("File content: " + encryptedHEXStringFileContent + "\n");

            byte[] byteFormat_enc_filecontent = hexStringToByteArray(encryptedHEXStringFileContent);
            string enc_File = Encoding.Default.GetString(byteFormat_enc_filecontent);
            //filename
            int indexV = message.IndexOf("V");
            int indexT = message.IndexOf("T");

            int length2 = indexV - indexP - 1;
            string fileName = message.Substring(indexP + 1, length2);

            //richTextBox1.AppendText("Filename: " + fileName + "\n");
            byte[] byteFormat_filename = hexStringToByteArray(fileName);
            string enc_FileName = Encoding.Default.GetString(byteFormat_filename);



            //HMAC
            int length3 = indexT - indexV - 1;
            string HMACedfile = message.Substring(indexV + 1, length3);

            //richTextBox1.AppendText("HMAC File: " + HMACedfile + "\n");
            byte[] byteHMACedfile = hexStringToByteArray(HMACedfile);
            string received_fileHMAC = Encoding.Default.GetString(byteHMACedfile);

            //HMAC filename
            int length4 = message.Length - indexT - 1;
            string HMACedfilename = message.Substring(indexT + 1, length4);

            //richTextBox1.AppendText("HMAC Filename: " + HMACedfilename + "\n");
            byte[] byteHMACedfilename = hexStringToByteArray(HMACedfilename);
            string received_filenameHMAC = Encoding.Default.GetString(byteHMACedfilename);

            bool check = true;

            try
            {
                richTextBox1.AppendText("Decrypting replicated file... \n");
                byte[] decrypted_filename = decryptWithAES128(enc_FileName, myAESkey, myIV);
                dec_filename = Encoding.Default.GetString(decrypted_filename);
                byte[] decrypted_file = decryptWithAES128(enc_File, myAESkey, myIV);
                dec_file = Encoding.Default.GetString(decrypted_file);

                byte[] HMACofFile = applyHMACwithSHA256(dec_file, myHMACkey);
                string myfileHMAC = Encoding.Default.GetString(HMACofFile);
                byte[] HMACofFileName = applyHMACwithSHA256(dec_filename, myHMACkey);
                string myfilenameHMAC = Encoding.Default.GetString(HMACofFileName);
                if ((myfileHMAC == received_fileHMAC) && (received_filenameHMAC == myfilenameHMAC))
                {
                    richTextBox1.AppendText("File received correctly. Sending signature and saving file... \n");
                }
                //Send signature and save the file

                else
                {
                    richTextBox1.AppendText("HMACs not the same... \n");
                    check = false;
                    string errorMessage = "HMACError";
                    byte[] signature = signWithRSA(errorMessage, 3072, xml3072_bit_key_Master);
                    string messageToSent = errorMessage + generateHexStringFromByteArray(signature);
                    Byte[] buffer8 = Encoding.Default.GetBytes(messageToSent);
                    s.Send(buffer8);
                    //signed("hmac error")+"hmac error"; send this
                    //alan taraf message=hmac error"---> verfiy signature ve log. 
                }

            }
            catch
            {
                richTextBox1.AppendText("Decryption of replicated file failed.\n");
                Byte[] buffer9 = Encoding.Default.GetBytes("Master failed decryption." + dec_filename);
                s.Send(buffer9);
                check = false;
            }

            if (check)
            {
                //Storee
                //signature, server signs file content with priv key
                //client verifs with public key
                byte[] signature = signWithRSA(dec_file, 3072, xml3072_bit_key_Master);
                string messageToSent = "verificationSignature" + generateHexStringFromByteArray(signature) + "P" + string_dec_filename; 
                //richTextBox1.AppendText("repfilecontent: " + dec_file + "\n");

                Byte[] buffer4 = Encoding.Default.GetBytes(messageToSent);
                s.Send(buffer4);

                //string filenameofverif = "filenameofverif:" + dec_filename;
                //Byte[] buffer5 = Encoding.Default.GetBytes(filenameofverif);
                //s.Send(buffer5);

                //// DOSYA OLUŞTURMA KISMI

                string folderName = @"c:\MasterServer";
                

                string pathString = System.IO.Path.Combine(folderName, "Files");

                System.IO.Directory.CreateDirectory(pathString);

                pathString = System.IO.Path.Combine(pathString, dec_filename);

                byte[] file_content = Encoding.Default.GetBytes(dec_file);

                using (System.IO.FileStream fs = System.IO.File.Create(pathString))
                {
                    fs.Write(file_content, 0, file_content.Length);
                }


            }


        }

        public static byte[] hexStringToByteArray(string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
        static byte[] decryptWithRSA(string input, int algoLength, string xmlStringKey)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);
            // create RSA object from System.Security.Cryptography
            RSACryptoServiceProvider rsaObject = new RSACryptoServiceProvider(algoLength);
            // set RSA object with xml string
            rsaObject.FromXmlString(xmlStringKey);
            byte[] result = null;

            try
            {
                result = rsaObject.Decrypt(byteInput, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }

        static byte[] decryptWithAES128(string input, byte[] key, byte[] IV)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);

            // create AES object from System.Security.Cryptography
            RijndaelManaged aesObject = new RijndaelManaged();
            // since we want to use AES-128
            aesObject.KeySize = 128;
            // block size of AES is 128 bits
            aesObject.BlockSize = 128;
            // mode -> CipherMode.*
            aesObject.Mode = CipherMode.CFB;
            // feedback size should be equal to block size
            // aesObject.FeedbackSize = 128;
            // set the key
            aesObject.Key = key;
            // set the IV
            aesObject.IV = IV;
            // create an encryptor with the settings provided
            ICryptoTransform decryptor = aesObject.CreateDecryptor();
            byte[] result = null;

            try
            {
                result = decryptor.TransformFinalBlock(byteInput, 0, byteInput.Length);
            }
            catch (Exception e) // if encryption fails
            {
                Console.WriteLine(e.Message); // display the cause
            }

            return result;
        }
        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            listening = false;
            terminating = true;
            Environment.Exit(0);
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
        // RSA encryption with varying bit length
        static byte[] encryptWithRSA(string input, int algoLength, string xmlStringKey)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);
            // create RSA object from System.Security.Cryptography
            RSACryptoServiceProvider rsaObject = new RSACryptoServiceProvider(algoLength);
            // set RSA object with xml string
            rsaObject.FromXmlString(xmlStringKey);
            byte[] result = null;

            try
            {
                //true flag is set to perform direct RSA encryption using OAEP padding
                result = rsaObject.Encrypt(byteInput, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }

        // signing with RSA
        static byte[] signWithRSA(string input, int algoLength, string xmlString)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);
            // create RSA object from System.Security.Cryptography
            RSACryptoServiceProvider rsaObject = new RSACryptoServiceProvider(algoLength);
            // set RSA object with xml string
            rsaObject.FromXmlString(xmlString);
            byte[] result = null;

            try
            {
                result = rsaObject.SignData(byteInput, "SHA256");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }
        static string generateHexStringFromByteArray(byte[] input)
        {
            string hexString = BitConverter.ToString(input);
            return hexString.Replace("-", "");
        }

        static byte[] encryptWithAES128(string input, byte[] key, byte[] IV)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);

            // create AES object from System.Security.Cryptography
            RijndaelManaged aesObject = new RijndaelManaged();
            // since we want to use AES-128
            aesObject.KeySize = 128;
            // block size of AES is 128 bits
            aesObject.BlockSize = 128;
            // mode -> CipherMode.*
            aesObject.Mode = CipherMode.CFB; //or OFB
            // feedback size should be equal to block size
            aesObject.FeedbackSize = 128;
            // set the key
            aesObject.Key = key;
            // set the IV
            aesObject.IV = IV;
            // create an encryptor with the settings provided
            ICryptoTransform encryptor = aesObject.CreateEncryptor();
            byte[] result = null;

            try
            {
                result = encryptor.TransformFinalBlock(byteInput, 0, byteInput.Length);
            }
            catch (Exception e) // if encryption fails
            {
                Console.WriteLine(e.Message); // display the cause
            }

            return result;
        }
        static byte[] applyHMACwithSHA256(string input, byte[] key)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);
            // create HMAC applier object from System.Security.Cryptography
            HMACSHA256 hmacSHA256 = new HMACSHA256(key);
            // get the result of HMAC operation
            byte[] result = hmacSHA256.ComputeHash(byteInput);

            return result;
        }
        public static bool IsEmpty<T>(List<T> list)
        {
            if (list == null)
            {
                return true;
            }

            return !list.Any();
        }
    }
}
