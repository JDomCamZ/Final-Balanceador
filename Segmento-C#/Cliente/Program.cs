using System;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace Cliente
{
    class Program
    {
        static void Main(string[] args)
        {
            // Establece el punto de entrada final para el socket.
            // Dns.GetHostName devuelve el nombre del host donde se ejecuta el código.
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 4444);


            try {
                // Crea un socket TCP/IP.
                Socket sender = new Socket(ipAddress.AddressFamily,SocketType.Stream, ProtocolType.Tcp);
                // Conecta el socket al punto de conexión remoto.
                sender.Connect(remoteEP);
                Console.WriteLine("Socket conectado a {0}",sender.RemoteEndPoint.ToString());
                //Console.WriteLine("ingrese texto");
                //string texto = Console.ReadLine();
                // Encodea el mensaje en un array de bytes.
                //byte[] msg = Encoding.ASCII.GetBytes(texto+"<EOF>");//como tiene el <EOF> entonces el servidor terminra´
                //int byteSent=sender.Send(msg);
                //sender.Shutdown(SocketShutdown.Both);
                //sender.Close();
                string comando = "";
                while (comando != "exit")
                {
                    Console.WriteLine("ingrese una cadena");
                    //enviando mensaje
                    comando = Console.ReadLine();
                    byte[] msg = Encoding.ASCII.GetBytes(comando + "<EOF>");//como tiene el <EOF> entonces el servidor terminra´
                    int byteSent=sender.Send(msg);



                    //reciviendo mensaje 
                    byte[] bytes = new byte[1024];
                    int byteRec = sender.Receive(bytes);
                    string texto = Encoding.ASCII.GetString(bytes, 0, byteRec);
                    Console.WriteLine("Servidor:" + texto);
                }
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }

            
        }
    }
}
