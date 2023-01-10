using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using System.Collections.Generic;
namespace Segmento
{
    
    class Program
    {
        static int[] limite = { 1, 10 };
        static Queue<string> pedidos= new Queue<string>();
        static void Main(string[] args)

        {   
            
            //datos necesario
            List<string[]> bd = new List<string[]>();
            //para iniciar la conexión
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = host.AddressList[0];//192.168.0.18
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 4444);

            //replica
            //IPAddress ipReplica = IPAddress.Parse("192.168.0.18");//192.168.0.18
            //IPEndPoint remoteReplica = new IPEndPoint(ipReplica, 5000);
            //Socket senderReplica = new Socket(ipReplica.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            //senderReplica.Connect(remoteReplica);
            

            LeerCSV(bd);
            try
            {
                // Crea un socket TCP/IP.
                Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                // Conecta el socket al punto de conexión remoto.
                sender.Connect(remoteEP);
                //senderReplica.Connect(remoteReplica);

                Console.WriteLine("Socket conectado a {0}", sender.RemoteEndPoint.ToString());
                

                Thread threadSend = new Thread(new ParameterizedThreadStart(Enviar));
                threadSend.Start(sender);

                Console.WriteLine("Reciviendo mensaje");

                // Aquí recive los mensajes
                while (true)
                {
                    byte[] bytes = new byte[1024];
                    int byteRec = sender.Receive(bytes);
                    string texto = Encoding.ASCII.GetString(bytes, 0, byteRec);
                    Console.WriteLine("Servidor:" + texto);
                    //Operaciones(texto,bd,sender,senderReplica);
                    string visible = texto.Replace("\r\n","");
                    if (visible == "Is Segment or Client?")
                    {
                        
                        EnviarMensaje(sender, "Segment");

                    }
                    else {
                        Op(visible, bd, sender);
                    }
                    
                }

                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /*static void Recivir(Object sender)
        {
            Socket s=(Socket)sender;
            Console.WriteLine("Reciviendo mensaje");

            // Aquí puedes poner el código que quieras ejecutar en el hilo
            while (true) {
                byte[] bytes = new byte[1024];
                int byteRec = s.Receive(bytes);
                string texto = Encoding.ASCII.GetString(bytes, 0, byteRec);
                Console.WriteLine("Servidor:" + texto);

            }
        }*/
        static void Enviar(Object sender) {
            Socket s = (Socket)sender;
            Console.WriteLine("Enviando mensajes");
            string comando = "";
            while (comando != "exit")
            {
                Console.WriteLine("ingrese una cadena");

                //enviando mensaje
                comando = Console.ReadLine();
                byte[] msg = Encoding.ASCII.GetBytes(comando + "<EOF>");//como tiene el <EOF> entonces el servidor terminra´
                int byteSent = s.Send(msg);

            }
        }
        static void EnviarMensaje(Socket sender,string mensaje)
        {
            Console.WriteLine("Enviando mensajes");
            //byte[] msg = Encoding.ASCII.GetBytes(mensaje + "<EOF>");
            byte[] msg = Encoding.ASCII.GetBytes(mensaje + "\n");
            int byteSent = sender.Send(msg);
        }
        static void EnviarReplica(Socket replica, string mensaje)
        {
            Console.WriteLine("Enviando replica");
            byte[] msg = Encoding.ASCII.GetBytes(mensaje);
            int byteSent = replica.Send(msg);
        }
        static void Operaciones(string texto, List<string[]>datos,Socket sender,Socket repli) {
                string[] partesOperación = texto.Split("-");
                if (partesOperación.Length > 1)
                {
                    if (partesOperación[0] == "B")
                    {
                        int min = int.Parse(partesOperación[1]);
                        int max = int.Parse(partesOperación[2]);
                        limite[0] = min;
                        limite[1] = max;
                        
                        //EnviarMensaje(repli, texto);
                    }
                    if (partesOperación[0] == "L"){
                            string dinero = "";
                            string resultado = "";
                            string busca = partesOperación[1];
                            foreach (string[] dato in datos)
                            {
                                if (dato[0] == busca) dinero = dato[1];
                            }
                            resultado = "S--L-" + busca + "-" + dinero + partesOperación[2];
                            EnviarMensaje(sender, resultado);
                    }else if (partesOperación[0] == "A"){
                            string[] partesActualizar = partesOperación[2].Split(";");
                            float monto = float.Parse(partesActualizar[2]);
                            string ordenante = partesActualizar[0];
                            string beneficiario= partesActualizar[1];
                            foreach (string[] dato in datos)
                            {
                                if (dato[0] == ordenante)
                                {
                                    float dineroActual = float.Parse(dato[1]);
                                    float resultadoDinero = (dineroActual - monto);
                                    if (resultadoDinero < 1000)
                                    {
                                        string resultado = "S--A-D-" + ordenante + "-"+ partesOperación[2];
                                        Console.WriteLine(resultado);
                                        EnviarMensaje(sender, resultado);
                                    }
                                    else
                                    {
                                        string resultado = "S--A-A-" + ordenante + "-" +resultadoDinero.ToString() + "-" +beneficiario+"-"+partesActualizar[2] + "-" + partesOperación[2];
                                        Console.WriteLine(resultado);
                                        EnviarMensaje(sender, resultado);
                                    }
                                }
                            }

                    }else if (partesOperación[0] == "R"){
                            string[] Actualizar = partesOperación[1].Split(";");
                            string beneficiario = Actualizar[0];
                            float dineroDepositado = float.Parse(Actualizar[1]);

                         foreach (string[] dato in datos){
                           if (dato[0] ==beneficiario) {
                            float dinero = float.Parse(dato[1]);
                            float dineroActualizado = dinero + dineroDepositado;

                            dato[1] = dineroActualizado.ToString();
                            Escribir(dato[0] + ";" + dato[1], int.Parse(dato[0]));

                            string resultado = "S--R-" + beneficiario + "-"+(dineroActualizado.ToString())+"-" + partesOperación[2];
                            Console.WriteLine(resultado);
                            EnviarMensaje(sender, resultado);
                            //enviar a replica
                            
                            //EnviarMensaje(repli, texto);
                        }
                          }
                        

                    }

                }
  
        }

        static void Op(string texto, List<string[]> datos, Socket sender)
        {
            string[] partesOperación = texto.Split("-");
            if (partesOperación.Length > 1)
            {
                if (partesOperación[0] == "B")
                {
                    int min = int.Parse(partesOperación[1]);
                    int max = int.Parse(partesOperación[2]);
                    limite[0] = min;
                    limite[1] = max;

                    //EnviarMensaje(repli, texto);
                }
                if (partesOperación[0] == "L")
                {
                    string dinero = "";
                    string resultado = "";
                    string busca = partesOperación[1];
                    foreach (string[] dato in datos)
                    {
                        if (dato[0] == busca) dinero = dato[1];
                    }
                    resultado = "S--L-" + busca + "-" + dinero + partesOperación[2];
                    EnviarMensaje(sender, resultado);
                }
                else if (partesOperación[0] == "A")
                {
                    string[] partesActualizar = partesOperación[1].Split(";");
                    float monto = float.Parse(partesActualizar[2]);
                    string ordenante = partesActualizar[0];
                    string beneficiario = partesActualizar[1];
                    foreach (string[] dato in datos)
                    {
                        if (dato[0] == ordenante)
                        {
                            float dineroActual = float.Parse(dato[1]);
                            float resultadoDinero = (dineroActual - monto);
                            if (resultadoDinero < 1000)
                            {
                                string resultado = "S--A-D-" + ordenante + "-" + partesOperación[2];
                                Console.WriteLine(resultado);
                                EnviarMensaje(sender, resultado);
                            }
                            else
                            {
                                string resultado = "S--A-A-" + ordenante + "-" + resultadoDinero.ToString() + "-" + beneficiario + "-" + partesActualizar[2] + "-" + partesOperación[2];
                                Console.WriteLine(resultado);
                                EnviarMensaje(sender, resultado);
                            }
                        }
                    }

                }
                else if (partesOperación[0] == "R")
                {
                    string[] Actualizar = partesOperación[1].Split(";");
                    string beneficiario = Actualizar[0];
                    float dineroDepositado = float.Parse(Actualizar[1]);

                    foreach (string[] dato in datos)
                    {
                        if (dato[0] == beneficiario)
                        {
                            float dinero = float.Parse(dato[1]);
                            float dineroActualizado = dinero + dineroDepositado;

                            dato[1] = dineroActualizado.ToString();
                            Escribir(dato[0] + ";" + dato[1], int.Parse(dato[0]));

                            string resultado = "S--R-" + beneficiario + "-" + (dineroActualizado.ToString()) + "-" + partesOperación[2];
                            Console.WriteLine(resultado);
                            EnviarMensaje(sender, resultado);
                            //enviar a replica

                            //EnviarMensaje(repli, texto);
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
                    if (cont >= limite[0] && cont <= limite[1]) {
                        Console.WriteLine("Valores: " + valores[0] + ", " + valores[1]);
                        lista.Add(new string[] { valores[0], valores[1] });
                    }
                    
                    if (cont == limite[1]) break;
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
