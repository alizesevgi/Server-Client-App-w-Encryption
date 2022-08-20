using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Threading;
using System.Security.Cryptography;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;


namespace client_432project
{

    public partial class Form1 : Form
    {
        bool terminating = false;
        bool connected = false;
        Socket clientSocket;
        string rsa_master_pub_key = "" ;
        string rsa_server_pub_key = "" ;
        string fileContent = string.Empty;
        string fileName = string.Empty;
        string clientName = "";
        string server_to_send;
        public static string server_key;
        string rsa;
        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();
        }

        private void connect_button_Click(object sender, EventArgs e)
        {

            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            string ip = IP_address_text.Text;
            string portNum = port_num_text.Text;
            clientName = clientName_box.Text;
            int port_num;
            if (Int32.TryParse(portNum, out port_num))
            {
                try
                {
                    clientSocket.Connect(ip, port_num);
                    connect_button.Enabled = false;
                    disconnect_button.Enabled = true;
                    button1_upload.Enabled = true;
                    download_button.Enabled = true;

                    connected = true;
                    richTextBox1.AppendText("Connected to the server. \n");

                    string username = "client";

                    Byte[] buffer = Encoding.Default.GetBytes(username);

                    clientSocket.Send(buffer);

                    Thread receiveThread = new Thread(new ThreadStart(Receive));
                    receiveThread.Start();

                }
                catch
                {
                    richTextBox1.AppendText("Could not connect to the server. \n");
                }

            }
            else
            {
                richTextBox1.AppendText("Check the port number. \n");
            }

        }

        private void Receive()
        {
            while (connected)
            {
                try
                {
                    Byte[] buffer = new Byte[2048576];
                    clientSocket.Receive(buffer);

                    string message = Encoding.Default.GetString(buffer);
                    message = message.Substring(0, message.IndexOf("\0"));

                    if (message == "You have connected to the MASTER")
                    {

                        using (System.IO.StreamReader fileReader =
                        new System.IO.StreamReader("MasterServer_pub.txt"))
                        {
                            rsa_server_pub_key = fileReader.ReadLine();
                        }
                        richTextBox1.AppendText("I have successfully connected to Master. \n");
                    }
                    else if (message == "You have connected to the SERVER1")
                    {

                        using (System.IO.StreamReader fileReader =
                        new System.IO.StreamReader("Server1_pub.txt"))
                        {
                            rsa_server_pub_key = fileReader.ReadLine();
                        }
                        richTextBox1.AppendText("I have successfully connected to Server 1.\n");
                    }
                    else if (message == "You have connected to the SERVER2")
                    {
                        using (System.IO.StreamReader fileReader =
                        new System.IO.StreamReader("Server2_pub.txt"))
                        {
                            rsa_server_pub_key = fileReader.ReadLine();
                        }
                        richTextBox1.AppendText("I have successfully connected to Server 2.\n");

                    }
                    else if (message.Length>21 && message.Substring(0,21) == "verificationSignature")
                    {
                        richTextBox1.AppendText("Server sent me its signature \n");
                        Thread verifySignatureThread = new Thread(() => verifySignature(clientSocket, message));
                        verifySignatureThread.Start();
                    }
                    else if (message.Length > 16 && message.Substring(0, 16) == "ResponseDownload")
                    {
                        richTextBox1.AppendText("Server sent me a response regarding my download request .. checking... \n");
                        Thread verifyDownloadThread = new Thread(() => verifyDownload(clientSocket, message));
                        verifyDownloadThread.Start();
                    }
                    else
                        richTextBox1.AppendText(message + "\n");

  
                }
                catch
                {
                    if (!terminating)
                    {
                        richTextBox1.AppendText("Connection has lost with the server. \n");
                    }

                    clientSocket.Disconnect(connected = false);
                    connected = false;
                    connect_button.Enabled = true;
                    disconnect_button.Enabled = false;
                    button1_upload.Enabled = false;
                    download_button.Enabled = false;
                }
            }
        }
        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            connected = false;
            terminating = true;
            Environment.Exit(0);
        }

        private void disconnect_button_Click(object sender, EventArgs e)
        {
            connected = false;
            clientSocket.Disconnect(connected = false);
            terminating = true;
            connect_button.Enabled = true;
            disconnect_button.Enabled = false;
            button1_upload.Enabled = false;
            download_button.Enabled = false;

            richTextBox1.AppendText("\n" + "You have successfully disconnected from the server.\n");
        }
      

        private byte[] File_Enc(string inputFile, string filename, string rsa_server_pub_key)
        {
            

            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            byte[] byteArray = new byte[16];
            provider.GetBytes(byteArray);
            string AES_Key = Encoding.Default.GetString(byteArray);

            RNGCryptoServiceProvider provider2 = new RNGCryptoServiceProvider();
            byte[] byteArray2 = new byte[16];
            provider2.GetBytes(byteArray2);
            string iv_Key = Encoding.Default.GetString(byteArray2);


            byte[] encrypted_AES_key = encryptWithRSA(AES_Key, 3072, rsa_server_pub_key);
            string encryptedString_AES_key = Encoding.Default.GetString(encrypted_AES_key);
            string encryptedHEXString_AES_key = generateHexStringFromByteArray(encrypted_AES_key);


            byte[] encrypted_iv_key = encryptWithRSA(iv_Key, 3072, rsa_server_pub_key);
            string encryptedString_iv_key = Encoding.Default.GetString(encrypted_iv_key);
            string encryptedHEXString_iv_key = generateHexStringFromByteArray(encrypted_iv_key);

            //richTextBox1.AppendText("File content is: " + inputFile + "\n");
            byte[] encryptedFileWithAES = encryptWithAES128(inputFile, byteArray, byteArray2);
            string encryptedHEXStringFileContent = generateHexStringFromByteArray(encryptedFileWithAES);

            richTextBox1.AppendText("Filename is: " + filename + "\n");
            byte[] encryptedFileNameWithAES = encryptWithAES128(filename, byteArray, byteArray2);
            string encryptedHEXStringFileName = generateHexStringFromByteArray(encryptedFileNameWithAES);

            string message_to_be_sent = "filecoming" + encryptedHEXStringFileContent + "P" + encryptedHEXStringFileName + "V" + encryptedHEXString_AES_key + "T" + encryptedHEXString_iv_key;
            richTextBox1.AppendText("I am sending a file to the server. \n");

            return Encoding.Default.GetBytes(message_to_be_sent);
           

        }
        void verifySignature(Socket s, string message)
        {
            int startPos = message.LastIndexOf("verificationSignature") + 21;
            int indexK = message.IndexOf("P");
            int length = indexK - startPos;
            string Signature = message.Substring(startPos, length);
            byte[] signature = hexStringToByteArray(Signature);

            

            bool verificationResult = verifyWithRSA(fileContent, 3072, rsa_server_pub_key, signature);
            if (verificationResult == true)
            {
                richTextBox1.AppendText("Valid signature, server received my file \n");
                
            }
            else
            {
                richTextBox1.AppendText("Invalid signature \n");
            }


        }
        void verifyDownload(Socket s, string message)
        {
            int startPos = 16;
            int indexP = message.IndexOf("P");
            int length = indexP - startPos;
            string Signature = message.Substring(startPos, length);
            byte[] signature = hexStringToByteArray(Signature);


            int indexV = message.IndexOf("V");


            int length2 = indexV - indexP - 1;
            string downloaded_file_content = message.Substring(indexP + 1, length2);
            byte[] byteFormat_downloaded_content = hexStringToByteArray(downloaded_file_content);
            string file_content_down = Encoding.Default.GetString(byteFormat_downloaded_content);

            int length3 = message.Length - indexV - 1;
            string downloaded_file_name = message.Substring(indexV + 1, length3);

            if (file_content_down == "FileNotFound")
            {
                bool verificationResult = verifyWithRSA(file_content_down, 3072, rsa_server_pub_key, signature);
                if (verificationResult == true)
                {
                    richTextBox1.AppendText("File not found messsage is received with valid signature. \n");
                }
                else
                {
                    richTextBox1.AppendText("File not found message is received with invalid signature \n");

                }

            }

            else
            {

                bool verificationResult = verifyWithRSA(file_content_down, 3072, rsa_server_pub_key, signature);
                if (verificationResult == true)
                {
                    richTextBox1.AppendText("Valid signature, server sent my file. I stored it. \n");
                    string filepath = @"c:\";
                    string folderName = System.IO.Path.Combine(filepath, clientName);
                    string pathString = System.IO.Path.Combine(folderName, "Files");

                    System.IO.Directory.CreateDirectory(pathString);

                    pathString = System.IO.Path.Combine(pathString, downloaded_file_name);

                    byte[] file_content = byteFormat_downloaded_content;

                    using (System.IO.FileStream fs = System.IO.File.Create(pathString))
                    {
                        fs.Write(file_content, 0, file_content.Length);
                    }

                }
                else
                {
                    richTextBox1.AppendText("File received but with invalid signature \n");
                }
            }


        }
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
        private void button1_upload_Click(object sender, EventArgs e)
        {
            
            var filePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;
                    textBox1_filepath.AppendText(filePath);

                    fileName = Path.GetFileName(filePath);

                    //READ FILE
                    byte[] content = File.ReadAllBytes(filePath);
                    fileContent = Encoding.Default.GetString(content);

                    Byte[] buffer4 = File_Enc(fileContent, fileName, rsa_server_pub_key);
                    clientSocket.Send(buffer4);
                    
                }
            }

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
        static string generateHexStringFromByteArray(byte[] input)
        {
            string hexString = BitConverter.ToString(input);
            return hexString.Replace("-", "");
        }

        public static byte[] hexStringToByteArray(string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
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

        private void download_button_Click(object sender, EventArgs e)
        {
            {
                string filename_to_download = download_file_name.Text;
                string download_message = "download" + filename_to_download;
                Byte[] buffer = Encoding.Default.GetBytes(download_message);

                clientSocket.Send(buffer);
                richTextBox1.AppendText("I have send a download request for the file " + filename_to_download + "\n");

            }

        }
    }
}
