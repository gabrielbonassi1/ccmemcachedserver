using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

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

        while(true) {
            TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine("Client connected");

            // lendo dados do cliente
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"Received message: {message}");

            // respondendo
            string answer = "Message Received";
            byte[] answerBytes = Encoding.UTF8.GetBytes(answer);
            stream.Write(answerBytes, 0, answerBytes.Length);

            client.Close();
        }
    }
}
