using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections;

class Program {
    static void Main(string[] args) {
        int port = 0;
        if (args.Length == 0) { // conecta na porta padrao 11211
            port = 11211;
            Console.WriteLine($"Connecting on default port: {port}");
        } else {
            string command = args[0].ToLower();
            switch (command) {
                case "-p":
                    if (int.TryParse(args[1], out port)) {
                        Console.WriteLine($"Connecting on port: {port}");
                    } else {
                        Console.WriteLine("ERROR: Invalid port");
                        return;
                    }
                    break;
                default:
                    Console.WriteLine($"Unknown command: {command}");
                    return;
            }
        }

        // criando socket
        var listener = new TcpListener(IPAddress.Loopback, port);
        listener.Start();
        Console.WriteLine("TCP Server awaiting connections");
        Hashtable memcache = new Hashtable();

        while(true) {
            TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine("Client connected");

            // lendo dados do cliente
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"Received Command: {message}");

            // parsing the message

            string[] cmd_parsed = message.Split(' ');
            // cmd_parsed[cmd_parsed.Length - 1] = cmd_parsed[cmd_parsed.Length - 1].Trim();
            string command = cmd_parsed[0].ToLower();
            string key = cmd_parsed[1].ToLower();
            ushort flags = 0;
            ushort exptime = 0;
            ushort bytecount = 0;
            string response;
            if (UInt16.TryParse(cmd_parsed[2].ToLower(), out flags) == false || UInt16.TryParse(cmd_parsed[3].ToLower(), out exptime) == false || UInt16.TryParse(cmd_parsed[4].ToLower(), out bytecount) == false) {
                Console.WriteLine("Error: wrong command format");
                response = "Error: wrong command format \r\n";
                byte[] errorAnswerBytes = Encoding.UTF8.GetBytes(response);
                stream.Write(errorAnswerBytes, 0, errorAnswerBytes.Length);

                client.Close();
                return;
            }
            
            switch (command) {
                case "set":
                    NetworkStream setStream = client.GetStream();
                    byte[] setBuffer = new byte[1024];
                    int setBytesRead = setStream.Read(setBuffer, 0, setBuffer.Length);
                    string setValue = Encoding.UTF8.GetString(setBuffer, 0, setBytesRead);
                    try {
                        memcache.Add(key, setValue);
                        response = "STORED \r\n";
                    } catch {
                        response = "END \r\n";
                    }
                    break;
                case "get":
                    if (memcache.ContainsKey(key)) {
                        response = "VALUE" + key + "\r\n" + memcache[key].ToString() + "\r\n" + "END";
                    } else {
                        response = "END \r\n";
                    }
                    break;
                default:
                    response = "END \r\n";
                    break;
            }


            // send answer
            byte[] responseBytes = Encoding.UTF8.GetBytes(response);
            stream.Write(responseBytes, 0, responseBytes.Length);

            client.Close();
        }
    }
}
