using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Security.Cryptography;
using System.IO;

namespace server1_432project
{
    //SERVER1
    public partial class Form1 : Form
    {
        bool terminating = false;
        bool listening = false;
        bool masterConnected = false;
        string RSAPrivateKey3072_Server1;
        string string_dec_file = "";
        string string_dec_filename = "";
        string dec_filename = "";
        string dec_file = "";
        string fileContent = string.Empty;
        string rsaofserver;
        Socket server2;
        bool server2conn = false;
        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Socket masterSocket;
        List<Socket> clientSockets = new List<Socket>();
        List<string> filenamesqueue = new List<string>();
        List<string> senttoserver2 = new List<string>();
        List<string> senttomaster = new List<string>();
        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            listening = false;
            terminating = true;
            masterConnected = false;
            Environment.Exit(0);
        }

        private void button1_Click(object sender, EventArgs e) //connect button
        {

            masterSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            string IP = IP_text.Text;
            int port;
            if (Int32.TryParse(port_to_be_connected.Text, out port))
            {
                try
                {
                    masterSocket.Connect(IP, port);
                    masterConnected = true;
                    button1.Enabled = false;
                    richTextBox1.AppendText("Connected to master server. \n");
                    // Send ID as message 
                    string username = "Server 1";

                    if (server2conn == true)
                    {
                        string mes = "Server 1-serversconnected";

                        Byte[] buffer3 = Encoding.Default.GetBytes(mes);

                        masterSocket.Send(buffer3);

                    }
                    else
                    {


                        Byte[] buffer = Encoding.Default.GetBytes(username);

                        masterSocket.Send(buffer);
                    }



                    Thread receiveThread = new Thread(new ThreadStart(() => Receive_from_master(username)));
                    receiveThread.Start();




                }
                catch
                {
                    richTextBox1.AppendText("Could not connect to master server\n");
                }
            }
            else
            {
                richTextBox1.AppendText("Check the port\n");
            }

        }
        void CheckDupes()
        {
            List<string> server2dir = new List<string>();
            if (Directory.Exists(@"c:\Server2\Files\"))
            {
                if (Directory.GetFiles(@"c:\Server2\Files\").Length != 0)
                {

                    DirectoryInfo dir1 = new DirectoryInfo(@"c:\Server2\Files\");
                    FileInfo[] files1 = dir1.GetFiles();
                    foreach (FileInfo file in files1)
                    {
                        server2dir.Add(file.Name);
                    }
                }

            }
            List<string> masterdir = new List<string>();
            if (Directory.Exists(@"c:\MasterServer\Files\"))
            {
                if (Directory.GetFiles(@"c:\MasterServer\Files\").Length != 0)
                {


                    DirectoryInfo dirx = new DirectoryInfo(@"c:\MasterServer\Files\");
                    if (Directory.GetFiles(@"c:\MasterServer\Files\").Length != 0)
                    {
                        FileInfo[] filesx = dirx.GetFiles();
                        foreach (FileInfo file in filesx)
                        {
                            masterdir.Add(file.Name);
                        }

                    }
                }


            }
            List<string> server1dir = new List<string>();
            if (Directory.Exists(@"c:\Server1\Files\"))
            {
                if (Directory.GetFiles(@"c:\Server1\Files\").Length != 0)
                {

                    DirectoryInfo dir2 = new DirectoryInfo(@"c:\Server1\Files\");
                    FileInfo[] files2 = dir2.GetFiles();
                    foreach (FileInfo file in files2)
                    {
                        server1dir.Add(file.Name);
                    }

                    List<string> templist = new List<string>();
                    templist = server1dir.Except(masterdir).ToList();
                    templist.AddRange(server1dir.Except(server2dir).ToList());
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
        private void button2_Click(object sender, EventArgs e)
        {
            server2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            string IP = "localhost";
            int port;
            if (Int32.TryParse(port_server2.Text, out port))
            {
                try
                {
                    server2.Connect(IP, port);
                    server2conn = true;
                    button2.Enabled = false;
                    richTextBox1.AppendText("Connecting to server 2\n");
                    // Send ID as message
                    string username = "Server 1";

                    Byte[] buffer = Encoding.Default.GetBytes(username);

                    server2.Send(buffer);

                    string message = "Serverisconnected";

                    Byte[] buffer2 = Encoding.Default.GetBytes(message);

                    masterSocket.Send(buffer2);

                    Thread receive2Thread = new Thread(new ThreadStart(() => Receive_from_server(username)));
                    receive2Thread.Start();

                }
                catch
                {
                    richTextBox1.AppendText("Could not connect to remote server\n");
                }
            }
            else
            {
                richTextBox1.AppendText("Check the port\n");
            }


        }
        byte[] myAESkey = new byte[16];
        byte[] myIV = new byte[16];
        byte[] myHMACkey = new byte[16];
        string RSAPublicKey3072_master;
        string RSAPublicKey3072_Server2;
        private void Receive_from_master(string user)
        {
            while (masterConnected)
            {
                try
                {
                    Byte[] buffer = new Byte[2048576];
                    masterSocket.Receive(buffer);

                    string message = Encoding.Default.GetString(buffer);
                    message = message.Substring(0, message.IndexOf("\0"));



                    //Server 2 Pub

                    using (System.IO.StreamReader fileReader =
                    new System.IO.StreamReader("Server2_pub.txt"))
                    {
                        RSAPublicKey3072_Server2 = fileReader.ReadLine();
                    }

                    //Master Key

                    using (System.IO.StreamReader fileReader =
                    new System.IO.StreamReader("MasterServer_pub.txt"))
                    {
                        RSAPublicKey3072_master = fileReader.ReadLine();
                    }

                    if (message.Substring(0, 9) == "hiddenKey")
                    {
                        richTextBox1.AppendText("Master server has sent my session key \n");
                        verifyKey(masterSocket, message, user);
                        //Thread verifySessionKeyThread = new Thread(() => verifyKey(masterSocket, message, user));
                        //verifySessionKeyThread.Start();

                    }
                    else if (message.Length > 13 && message.Substring(0, 13) == "repfilecoming")
                    {
                        richTextBox1.AppendText("Master server sent me a file \n");
                        Thread receiverepThread = new Thread(() => ReceivefromMaster(masterSocket, "MASTER", message)); // updated
                        receiverepThread.Start();
                    }
                    else if (message.Length > 21 && message.Substring(0, 21) == "verificationSignature")
                    {
                        richTextBox1.AppendText("Master sent me its signature \n");
                        //Thread verifySignatureThread = new Thread(() => verifySignature("MASTER", message, fileContent));
                        //verifySignatureThread.Start();
                        verifySignature("MASTER", message, fileContent);
                    }
                    else if (message.Length > 9 && message.Substring(0, 9) == "HMACError")
                    {
                        richTextBox1.AppendText("Master sent me HMAC error message... Now verifying signature... \n");
                        verifyHMACError(masterSocket, message, "Master");

                    }
                    //else if (message.Length > 16 && message.Substring(0, 16) == "filenameofverif:")
                    //{
                    //    richTextBox1.AppendText(message);
                    //    int startPos = message.LastIndexOf("filenameofverif:") + 16;
                    //    int range = message.Length - startPos - 1;
                    //    string filenameofverif = message.Substring(startPos, range);
                    //    senttomaster.Add(filenameofverif);

                    //    if (senttoserver2.Contains(filenameofverif))
                    //    {
                    //        filenamesqueue.RemoveAll(name => name.Contains(filenameofverif));
                    //        //filenamesqueue.Remove(filenameofverif);
                    //    }

                    //}
                    else if (message.Length > 25 && message.Substring(0, 25) == "Master failed decryption.")
                    {
                        richTextBox1.AppendText("Master failed decryption.");
                        int startPos = message.LastIndexOf("Server 2 failed decryption.") + 25;
                        int range = message.Length - startPos - 1;
                        string filenameofverif = message.Substring(startPos, range);
                        if (!filenamesqueue.Contains(filenameofverif))
                        {
                            filenamesqueue.Add(filenameofverif);
                        }

                    }
                    else
                    {
                        richTextBox1.AppendText(message + "\n");
                    }

                }
                catch
                {
                    if (!terminating)
                    {
                        richTextBox1.AppendText("Connection has been lost with the server. \n");
                    }

                    masterSocket.Close();
                    masterConnected = false;
                    button1.Enabled = true;
                }
            }
        }
        void verifyHMACError(Socket s, string message, string server)
        {
            string errorMessage = message.Substring(0, 9);
            string signature = message.Substring(9);

            if (server == "Server 2")
            {
                rsaofserver = RSAPublicKey3072_Server2;
            }
            else if (server == "Master")
            {
                rsaofserver = RSAPublicKey3072_master;
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
        private void Receive_from_server(string user)
        {
            while (server2conn)
            {
                try
                {
                    Byte[] buffer = new Byte[2048576];
                    server2.Receive(buffer);

                    string message = Encoding.Default.GetString(buffer);
                    message = message.Substring(0, message.IndexOf("\0"));




                    if (message.Length > 13 && message.Substring(0, 13) == "repfilecoming")
                    {
                        richTextBox1.AppendText("Server 2 sent me a file \n");
                        Thread receiverep2Thread = new Thread(() => ReceivefromServer(server2, "Server 2", message)); // updated
                        receiverep2Thread.Start();
                    }
                    else if (message.Length > 21 && message.Substring(0, 21) == "verificationSignature")
                    {
                        richTextBox1.AppendText("Server 2 sent me its signature \n");
                        Task verifySignatureThread = new Task(() => verifySignature("Server 2", message, fileContent));
                        verifySignatureThread.Start();
                    }
                    else if (message.Length > 9 && message.Substring(0, 9) == "HMACError")
                    {
                        richTextBox1.AppendText("Master sent me HMAC error message... Now verifying signature... \n");
                        verifyHMACError(server2, message, "Server 2");

                    }
                    //else if (message.Length > 16 && message.Substring(0, 16) == "filenameofverif:")
                    //{
                    //    richTextBox1.AppendText(message);
                    //    int startPos = message.LastIndexOf("filenameofverif:") + 16;
                    //    int range = message.Length - startPos - 1;
                    //    string filenameofverif = message.Substring(startPos, range);
                    //    senttoserver2.Add(filenameofverif);


                    //}
                    else if (message.Length > 27 && message.Substring(0, 27) == "Server 2 failed decryption.")
                    {
                        richTextBox1.AppendText("Server 2 failed decryption.");
                        int startPos = message.LastIndexOf("Server 2 failed decryption.") + 27;
                        int range = message.Length - startPos - 1;
                        string filenameofverif = message.Substring(startPos, range);
                        filenamesqueue.Add(filenameofverif);
                    }
                    else
                    {
                        richTextBox1.AppendText(message + "\n");

                    }

                }
                catch
                {
                    if (!terminating)
                    {
                        richTextBox1.AppendText("Connection has lost with the server. \n");

                        string message = "Serverdisconnected";

                        Byte[] buffer2 = Encoding.Default.GetBytes(message);
                        if (masterConnected)
                        {
                            masterSocket.Send(buffer2);
                        }

                    }

                    server2.Close();
                    server2conn = false;
                    button2.Enabled = true;
                }
            }
        }
        async void verifySignature(string server, string message, string fileContent)
        {

            int startPos = message.LastIndexOf("verificationSignature") + 21;
            int indexK = message.IndexOf("P");
            int length = indexK - startPos;
            string Signature = message.Substring(startPos, length);
            byte[] signature = hexStringToByteArray(Signature);



            if (server == "MASTER")
            {
                rsaofserver = RSAPublicKey3072_master;
            }
            else if (server == "Server 2")
            {
                rsaofserver = RSAPublicKey3072_Server2;
            }
            bool verificationResult = verifyWithRSA(fileContent, 3072, rsaofserver, signature);

            if (message.IndexOf("P") + 1 != message.Length && verificationResult)
            {
                int length2 = message.Length - indexK - 1;
                string filename = message.Substring(indexK + 1, length2);

                if (server == "MASTER")
                {
                    senttomaster.Add(filename);
                }
                else if (server == "Server 2")
                {
                    senttoserver2.Add(filename);
                }
                await Task.Delay(2000);
                if (senttoserver2.Contains(filename) && senttomaster.Contains(filename))
                {
                    filenamesqueue.RemoveAll(name => name.Contains(filename));

                }

            }
            if (verificationResult == true)
            {
                richTextBox1.AppendText("Valid signature, server received my file \n");
                Byte[] buffer4 = Encoding.Default.GetBytes("Server1 verified my signature.");
                if (server == "Server 2")
                {

                    server2.Send(buffer4);
                }
                else if (server == "Master")
                {
                    masterSocket.Send(buffer4);
                }
            }
            else
            {
                richTextBox1.AppendText("Invalid signature \n");
            }


        }
        async void SendQueue()
        {
            if (server2conn && masterConnected && filenamesqueue.Any())
            {

                foreach (string filename in filenamesqueue.ToList())
                {
                    string filepath = @"c:\Server1\Files\";
                    string myPath = Path.Combine(filepath, filename);
                    if (File.Exists(myPath))
                    {
                        byte[] content = File.ReadAllBytes(myPath);
                        string tobesent_fileContent = Encoding.Default.GetString(content);

                        richTextBox1.AppendText("File found in queue... sending now... \n");

                        SendToServers(tobesent_fileContent, filename);
                        await Task.Delay(2000);


                    }
                }
            }
            else
            {
                foreach (string fuck in filenamesqueue.ToList())
                {
                    richTextBox1.AppendText(fuck + "\n");
                }



            }


        }
        private void verifyKey(Socket master, string message, string user)
        {

            richTextBox1.AppendText("entered verification \n");

            //hiddenKey encr P signature


            int startPos = message.LastIndexOf("hiddenKey") + 9;
            int indexK = message.IndexOf("P");
            int length = indexK - startPos;
            string encryptedKey = message.Substring(startPos, length);
            //richTextBox1.AppendText("Encrypted RSA:" + encryptedKey + "\n");

            byte[] byteFormat_enc_session = hexStringToByteArray(encryptedKey);
            string enc_session_key = Encoding.Default.GetString(byteFormat_enc_session);
            //richTextBox1.AppendText("encr session key:" + generateHexStringFromByteArray(byteFormat_enc_session));

            int length2 = message.Length - indexK - 1;
            string SignedKey = message.Substring(indexK + 1, length2);
            // richTextBox1.AppendText("Signed: " + SignedKey);
            byte[] byteFormat_signed_session = hexStringToByteArray(SignedKey);
            //richTextBox1.AppendText("signed session key:" + generateHexStringFromByteArray(byteFormat_signed_session));

            //Server 2 Pub

            using (System.IO.StreamReader fileReader =
            new System.IO.StreamReader("Server2_pub.txt"))
            {
                RSAPublicKey3072_Server2 = fileReader.ReadLine();
            }

            //Server 1 Prv

            using (System.IO.StreamReader fileReader =
            new System.IO.StreamReader("Server1_pub_prv.txt"))
            {
                RSAPrivateKey3072_Server1 = fileReader.ReadLine();
            }

            //Master Key

            using (System.IO.StreamReader fileReader =
            new System.IO.StreamReader("MasterServer_pub.txt"))
            {
                RSAPublicKey3072_master = fileReader.ReadLine();
            }

            byte[] session_key = decryptWithRSA(enc_session_key, 3072, RSAPrivateKey3072_Server1);
            string string_session_key = Encoding.Default.GetString(session_key);
            Array.Copy(session_key, myAESkey, 16);
            Array.Copy(session_key, 16, myIV, 0, 16);
            Array.Copy(session_key, 32, myHMACkey, 0, 16);

            bool verificationResult = verifyWithRSA(string_session_key, 3072, RSAPublicKey3072_master, byteFormat_signed_session);
            if (verificationResult == true)
            {
                richTextBox1.AppendText("I have received my session key securely \n");
                richTextBox1.AppendText("Encrypted session key: " + encryptedKey + "\n");
                CheckDupes();
            }
            else
            {
                richTextBox1.AppendText("I haven't received my session key securely \n");
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
        static string generateHexStringFromByteArray(byte[] input)
        {
            string hexString = BitConverter.ToString(input);
            return hexString.Replace("-", "");
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


        /// below is for listening clients ///////////

        private void listen_Click(object sender, EventArgs e)
        {
            int serverPort;

            if (Int32.TryParse(port_to_listen.Text, out serverPort))
            {

                serverSocket.Bind(new IPEndPoint(IPAddress.Any, serverPort));
                serverSocket.Listen(3);

                listening = true;
                listen.Enabled = false;



                Thread acceptThread = new Thread(Accept);
                acceptThread.Start();

                richTextBox1.AppendText("Started listening on port: " + serverPort + "\n");
                CheckDupes();

            }
            else
            {
                richTextBox1.AppendText("Please check port number \n");
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
                    richTextBox1.AppendText("A client is connected. \n");

                    //Byte[] buffer2 = Encoding.Default.GetBytes("You have connected to the SERVER1");
                    //newClient.Send(buffer2);


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

                    richTextBox1.AppendText("A user entered their role as; " + server_id + "\n");

                    richTextBox1.AppendText(server_id + " has successfully connected.\n");

                    if (server_id == "client")
                    {
                        Byte[] buffer2 = Encoding.Default.GetBytes("You have connected to the SERVER1");
                        thisClient.Send(buffer2);
                        Thread receiveThread = new Thread(() => Receive(thisClient, server_id)); // updated
                        receiveThread.Start();

                        //client knows which server it connectes
                    }
                    else if (server_id == "Server 2")
                    {
                        server2 = thisClient;
                        richTextBox1.AppendText("Can receive from and send to Server 2 \n");
                        server2conn = true;
                        Thread receivefromserverThread = new Thread(() => Receive_from_server(server_id)); // updated
                        receivefromserverThread.Start();


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
                    trying = false;
                }
            }
        }
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

                    if (message.Length > 10 && message.Substring(0, 10) == "filecoming")
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

                    connected = false;
                }


            }
        }
        private void ReceivefromMaster(Socket Mastersocket, string serverid, string message)
        {
            richTextBox1.AppendText("Master is sending me a file \n");
            verifyReplicate(Mastersocket, message);

        }
        private void ReceivefromServer(Socket server2, string serverid, string message)
        {
            richTextBox1.AppendText("Server2 is sending me a file \n");
            verifyReplicate(server2, message);

        }
        void verifyReplicate(Socket s, string message)
        {
            //if the connection has continued and file has been received fully
            //AES128 file
            int startPos = message.LastIndexOf("repfilecoming") + 13;
            int indexP = message.IndexOf("P");
            int length = indexP - startPos;
            string encryptedHEXStringFileContent = message.Substring(startPos, length);

            byte[] byteFormat_enc_filecontent = hexStringToByteArray(encryptedHEXStringFileContent);
            string enc_File = Encoding.Default.GetString(byteFormat_enc_filecontent);
            //filename
            int indexV = message.IndexOf("V");
            int indexT = message.IndexOf("T");

            int length2 = indexV - indexP - 1;
            string fileName = message.Substring(indexP + 1, length2);
            byte[] byteFormat_filename = hexStringToByteArray(fileName);
            string enc_FileName = Encoding.Default.GetString(byteFormat_filename);



            //HMAC
            int length3 = indexT - indexV - 1;
            string HMACedfile = message.Substring(indexV + 1, length3);
            byte[] byteHMACedfile = hexStringToByteArray(HMACedfile);
            string received_fileHMAC = Encoding.Default.GetString(byteHMACedfile);

            //HMAC filename
            int length4 = message.Length - indexT - 1;
            string HMACedfilename = message.Substring(indexT + 1, length4);
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
                    richTextBox1.AppendText("HMACs not same... \n");
                    //signed fail message
                    check = false;
                    string errorMessage = "HMACError";
                    byte[] signature = signWithRSA(errorMessage, 3072, RSAPrivateKey3072_Server1);
                    string messageToSent = errorMessage + generateHexStringFromByteArray(signature);
                    Byte[] buffer8 = Encoding.Default.GetBytes(messageToSent);
                    s.Send(buffer8);
                }

            }
            catch
            {
                richTextBox1.AppendText("Decryption of replicated file failed.\n");
                Byte[] buffer9 = Encoding.Default.GetBytes("Server 1 failed decryption." + dec_filename);
                s.Send(buffer9);
                check = false;
            }

            if (check)
            {
                //Storee
                //signature, server signs file content with priv key
                //client verifs with public key
                using (System.IO.StreamReader fileReader =
                new System.IO.StreamReader("Server1_pub_prv.txt"))
                {
                    RSAPrivateKey3072_Server1 = fileReader.ReadLine();
                }

                byte[] signature = signWithRSA(dec_file, 3072, RSAPrivateKey3072_Server1);
                string messageToSent = "verificationSignature" + generateHexStringFromByteArray(signature) + "P" + string_dec_filename; //filename şu anda bi sikim yapmıyor
                //richTextBox1.AppendText("repfilecontent: " + dec_file);//verifysignature'ı değiştiriceksin diye bıraktım buraları

                Byte[] buffer4 = Encoding.Default.GetBytes(messageToSent);
                s.Send(buffer4);

                //string filenameofverif = "filenameofverif:" + dec_filename;
                //Byte[] buffer5 = Encoding.Default.GetBytes(filenameofverif);
                //s.Send(buffer5);

                //// DOSYA OLUŞTURMA KISMI

                string folderName = @"c:\Server1";

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
        public async void SendToServers(string file, string filename)
        {

            //file and filename aes enc
            byte[] encryptedFileWithAES = encryptWithAES128(file, myAESkey, myIV);
            string encryptedHEXStringFileContent = generateHexStringFromByteArray(encryptedFileWithAES);
            byte[] encryptedFileNameWithAES = encryptWithAES128(filename, myAESkey, myIV);
            string encryptedHEXStringFileName = generateHexStringFromByteArray(encryptedFileNameWithAES);
            //hmac of file & filename
            byte[] HMACofFile = applyHMACwithSHA256(file, myHMACkey);
            string HMACofFileHEXString = generateHexStringFromByteArray(HMACofFile);
            byte[] HMACofFileName = applyHMACwithSHA256(filename, myHMACkey);
            string HMACofFileNameHEXString = generateHexStringFromByteArray(HMACofFileName);

            string file_to_be_sent = "repfilecoming" + encryptedHEXStringFileContent + "P" + encryptedHEXStringFileName + "V" + HMACofFileHEXString + "T" + HMACofFileNameHEXString;


            fileContent = file;
            Byte[] buffer6 = Encoding.Default.GetBytes(file_to_be_sent);
            if (masterConnected && server2conn && filenamesqueue.Contains(filename))
            {
                richTextBox1.AppendText("Sending a replicated file to servers. \n");
                if (!senttoserver2.Contains(filename))
                {
                    server2.Send(buffer6);
                    richTextBox1.AppendText("Server 2 has been sent replicate file. \n");
                }
                await Task.Delay(2000);
                if (!senttomaster.Contains(filename))
                {
                    masterSocket.Send(buffer6);
                    richTextBox1.AppendText("Master has been sent replicate file. \n");
                }

                filenamesqueue.Remove(filename);

            }
            else
            {
                richTextBox1.AppendText("File replication cannot occur due to offline servers. File queued. \n");
                //richTextBox1.AppendText("server2conn:" + server2conn + " masterconn:" + masterConnected);

            }



        }
        void Download_Function(Socket s, string message)
        {


            using (System.IO.StreamReader fileReader =
            new System.IO.StreamReader("Server1_pub_prv.txt"))
            {
                RSAPrivateKey3072_Server1 = fileReader.ReadLine();
            }
            //richTextBox1.AppendText("download request succesful");
            string filerequest = message.Substring(8);
            //richTextBox1.AppendText("\n file name:" + filerequest);
            string filepath = @"c:\Server1\Files\";
            string download_path = System.IO.Path.Combine(filepath, filerequest);
            //MessageBox.Show(filepath);

            if (System.IO.File.Exists(download_path))
            {
                richTextBox1.AppendText("Requested file found... sending now... \n");
                byte[] content = File.ReadAllBytes(download_path);
                string download_fileContent = Encoding.Default.GetString(content);
                //richTextBox1.AppendText(download_fileContent);


                byte[] signature = signWithRSA(download_fileContent, 3072, RSAPrivateKey3072_Server1);
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
                byte[] signature = signWithRSA(errormes, 3072, RSAPrivateKey3072_Server1);
                string messageToSent = "ResponseDownload" + generateHexStringFromByteArray(signature) + "P" + generateHexStringFromByteArray(errormessage) + "V" + filerequest;
                Byte[] buffer = Encoding.Default.GetBytes(messageToSent);
                s.Send(buffer);

            }
        }
        void verifyFile(Socket s, string message)
        {

            using (System.IO.StreamReader fileReader =
            new System.IO.StreamReader("Server1_pub_prv.txt"))
            {
                RSAPrivateKey3072_Server1 = fileReader.ReadLine();
            }

            //dummy string s= "filecomingAAAAPBBBVCCTD
            int startPos = message.LastIndexOf("filecoming") + 10;
            int indexP = message.IndexOf("P");
            int length = indexP - startPos;
            string encryptedHEXStringFileContent = message.Substring(startPos, length);

            byte[] byteFormat_enc_filecontent = hexStringToByteArray(encryptedHEXStringFileContent);
            string enc_FileContent = Encoding.Default.GetString(byteFormat_enc_filecontent);


            int indexV = message.IndexOf("V");
            int indexT = message.IndexOf("T");

            int length2 = indexV - indexP - 1;
            string fileName = message.Substring(indexP + 1, length2);
            byte[] byteFormat_filename = hexStringToByteArray(fileName);
            string enc_FileName = Encoding.Default.GetString(byteFormat_filename);




            int length3 = indexT - indexV - 1;
            string encryptedHEXString_AES_key = message.Substring(indexV + 1, length3);
            // richTextBox1.AppendText("Signed: " + SignedKey);
            byte[] byteFormat_AES_Key = hexStringToByteArray(encryptedHEXString_AES_key);
            string enc_AES_Key = Encoding.Default.GetString(byteFormat_AES_Key);


            int length4 = message.Length - indexT - 1;
            string encryptedHEXString_iv_key = message.Substring(indexT + 1, length4);
            byte[] byteFormat_iv_key = hexStringToByteArray(encryptedHEXString_iv_key);
            string enc_iv_Key = Encoding.Default.GetString(byteFormat_iv_key);
            bool check = true;

            try
            {
                byte[] decrypted_AES_Key = decryptWithRSA(enc_AES_Key, 3072, RSAPrivateKey3072_Server1);
                byte[] decrypted_iv = decryptWithRSA(enc_iv_Key, 3072, RSAPrivateKey3072_Server1);
                byte[] decrypted_filename = decryptWithAES128(enc_FileName, decrypted_AES_Key, decrypted_iv);
                byte[] decrypted_file = decryptWithAES128(enc_FileContent, decrypted_AES_Key, decrypted_iv);
                string_dec_file = Encoding.Default.GetString(decrypted_file);
                string_dec_filename = Encoding.Default.GetString(decrypted_filename);
                richTextBox1.AppendText("decryption complete..now sending the signature. \n");

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
                byte[] signature = signWithRSA(string_dec_file, 3072, RSAPrivateKey3072_Server1);

                string messageToSent = "verificationSignature" + generateHexStringFromByteArray(signature) + "P";
                //richTextBox1.AppendText("filecontent: " + string_dec_file);
                Byte[] buffer4 = Encoding.Default.GetBytes(messageToSent);
                s.Send(buffer4);

                //// DOSYA OLUŞTURMA KISMI

                string folderName = @"c:\Server1";

                string pathString = System.IO.Path.Combine(folderName, "Files");

                System.IO.Directory.CreateDirectory(pathString);

                pathString = System.IO.Path.Combine(pathString, string_dec_filename);

                byte[] file_content = Encoding.Default.GetBytes(string_dec_file);

                using (System.IO.FileStream fs = System.IO.File.Create(pathString))
                {
                    fs.Write(file_content, 0, file_content.Length);
                }
                if (senttoserver2.Contains(string_dec_filename))
                {
                    int index = senttoserver2.IndexOf(string_dec_file);
                    senttoserver2[index] = string_dec_file + "_old";
                }
                if (senttomaster.Contains(string_dec_filename))
                {
                    int index = senttomaster.IndexOf(string_dec_file);
                    senttomaster[index] = string_dec_file + "_old";
                }
                filenamesqueue.Add(string_dec_filename);
                SendToServers(string_dec_file, string_dec_filename);
                //Thread sendtoservers = new Thread(() => SendToServers(string_dec_file, string_dec_filename));
                //sendtoservers.Start();
            }




        }

        private void label2_Click(object sender, EventArgs e)
        {

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


    }



}



