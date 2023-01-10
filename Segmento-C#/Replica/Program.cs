using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using System.Collections.Generic;

namespace Replica
{
    class Program
    {
        static int[] limiteRe = { 1, 10 };
        static Queue<string> pedidosRe = new Queue<string>();
        static void Main(string[] args)
        {   


            
            //datos necesario
            List<string[]> bd = new List<string[]>();
            //para iniciar la conexión

            IPAddress ipReplica = IPAddress.Parse("192.168.0.18");//192.168.0.18
            IPEndPoint remoteReplica = new IPEndPoint(ipReplica, 5000);
            Socket senderReplica = new Socket(ipReplica.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            senderReplica.Connect(remoteReplica);

            LeerCSV(bd);
            try
            {
                Console.WriteLine("Socket conectado a {0}", senderReplica.RemoteEndPoint.ToString());
                Console.WriteLine("Reciviendo mensaje");

                // Arecivir
                while (true)
                {
                    byte[] bytes = new byte[1024];
                    int byteRec = senderReplica.Receive(bytes);
                    string texto = Encoding.ASCII.GetString(bytes, 0, byteRec);

                    Console.WriteLine(texto);
                    Operaciones(texto, bd, senderReplica);
                }

                senderReplica.Shutdown(SocketShutdown.Both);
                senderReplica.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static void EnviarMensaje(Socket sender,string mensaje)
        {
            Console.WriteLine("Enviando mensajes");
            Console.WriteLine("Enviando");
            byte[] msg = Encoding.ASCII.GetBytes(mensaje + "<EOF>");//como tiene el <EOF> entonces el servidor terminra´
            int byteSent = sender.Send(msg);
        }

        static void Operaciones(string texto, List<string[]>datos,Socket sender) {
                string[] partesOperación = texto.Split("-");
                if (partesOperación.Length > 1)
                {
                    if (partesOperación[0] == "B")
                    {
                        int min = int.Parse(partesOperación[0]);
                        int max = int.Parse(partesOperación[1]);
                        limiteRe[0] = min;
                        limiteRe[1] = max;
                        Console.WriteLine("Limite desde <"+limiteRe[0]+" "+limiteRe[1]);
                    }else if (partesOperación[0] == "R"){
                            string beneficiario =  partesOperación[1];
                            string dinero = partesOperación[2];

                         foreach (string[] dato in datos){
                           if (dato[0] ==beneficiario){//R-730-80.40-02
                            dato[0] = beneficiario;
                            dato[1] = dinero;
                            //Escribir(dato[0] + ";" + dato[1], int.Parse(dato[0]));
                            Console.WriteLine("replica de:  ID:"+beneficiario+"   dinero:"+dinero);
                            //EnviarMensaje(sender, resultado);
                            //enviar a replica
                            }
                          }
                        

                    }

                }
  
        }

        static void LeerCSV(List<string[]>lista)
        {
            // Abre el archivo CSV
            using (StreamReader reader = new StreamReader("bd.csv"))
            {
 

                // Lee el resto del archivo
                int cont = 0;
                while (!reader.EndOfStream)
                {
                    
                    string line = reader.ReadLine();
                    string[] valores = line.Split(';');

                    // Muestra los valores
                    if (cont >= limiteRe[0] && cont <= limiteRe[1]) {
                        Console.WriteLine("Valores: " + valores[0] + ", " + valores[1]);
                        lista.Add(new string[] { valores[0], valores[1] });
                    }
                    
                    if (cont == limiteRe[1]) break;
                    cont++;
                }
            }
        }

        static void Escribir(string s,int fila) {

            // Abre el archivo CSV en modo lectura
            using (var reader = new StreamReader("bd.csv"))
            {
                // Abre el archivo CSV en modo escritura
                using (var writer = new StreamWriter("bd.csv"))
                {
                    int count = 0;
                    // Recorre cada línea del archivo CSV
                    while(true)
                    {
                        string line = reader.ReadLine();

                        // Si es la fila que deseas sobrescribir, escribe los valores modificados
                        if (count == fila)
                        {
                            line = s;
                        }

                        // Escribe la línea en el archivo CSV
                        writer.WriteLine(line);
                    }
                }
            }

        }
    }
}
