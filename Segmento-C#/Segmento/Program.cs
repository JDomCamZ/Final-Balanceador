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
            

            //LeerCSV(bd);
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
                   // string visible = texto.Replace("\r\n", "");
                    string visible = "";
                    for(int i =0; i < texto.Length - 2; i++)
                    {
                        visible += texto[i].ToString();
                    }
                    Console.WriteLine("mensaje Servidor:" + visible);
                    //Operaciones(texto,bd,sender,senderReplica);
                    Console.WriteLine(limite[0] + " >-< " + limite[1]);
                    if (visible == "Is Segment or Client?")
                    {
                        Console.WriteLine("CONFIRMAR SI ES SEGMENTO");
                        EnviarMensaje(sender, "Segment");

                    }
                    else {
                        Console.WriteLine("VA A REALIZAR ");/////
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

        static void Op(string texto, List<string[]> datos, Socket sender)
        {
            Console.WriteLine("EMPEIZA A BUSCAR LA OPERACIÓN");
            string[] partesOperación = texto.Split("-");
            if (partesOperación.Length > 1)
            {
                Console.WriteLine("Hay mensaje que será revisaso");
                if (partesOperación[0] == "B")
                {   
                    int min = int.Parse(partesOperación[1]);
                    int max = int.Parse(partesOperación[2]);
                    limite[0] = min;
                    limite[1] = max;
                    LeerCSV(datos,min,max);
                    Console.WriteLine(limite[0]+" >-< "+limite[1]);
                    Console.WriteLine("Se balanceó correctamente");
                    //EnviarMensaje(repli, texto);
                }
                if (partesOperación[0] == "L")
                {
                    Console.WriteLine("Empieza lectura");
                    string dinero = "";
                    string resultado = "";
                    string busca = partesOperación[1];
                    foreach (string[] dato in datos)
                    {
                        Console.WriteLine(dato[0]);
                        Console.WriteLine(busca);
                        if (dato[0] == busca) {
                            Console.WriteLine("!!!!!!ENCONTRÓ en posicion" + dato[0]);
                            dinero = dato[1];
                            resultado = "S--L-" + busca + "-" + dinero + "-" + partesOperación[2];
                            EnviarMensaje(sender, resultado);
                            Console.WriteLine("L------------TERMINA LECTURA");
                            break; 
                        }
                        
                    }
                    
                }
                else if (partesOperación[0] == "A")
                {
                    Console.WriteLine("Empieza actualización ORDENANTE");
                    string[] partesActualizar = partesOperación[1].Split(";");
                    float monto = float.Parse(partesActualizar[2]);
                    string ordenante = partesActualizar[0];
                    string beneficiario = partesActualizar[1];
                    foreach (string[] dato in datos)
                    {
                        //Console.WriteLine("ENTRA AL FOR DE ACTUALIZACIÓN A");
                        if (dato[0] == ordenante)
                        {
                            //Console.WriteLine("Encuentra la actualización en "+dato[0]);
                            float dineroActual = float.Parse(dato[1]);
                            float resultadoDinero = (dineroActual - monto);
                            if (resultadoDinero < 1000)
                            {
                                string resultado = "S--A-D-" + ordenante + "-" + partesOperación[2];
                                Console.WriteLine(resultado);
                                EnviarMensaje(sender, resultado);
                                Console.WriteLine("ACTUALIZACIÓN DENEGADA ORDENANTE");
                            }
                            else
                            {
                                string resultado = "S--A-A-" + ordenante + "-" + resultadoDinero.ToString() + "-" + beneficiario + "-" + partesActualizar[2] + "-" + partesOperación[2];
                                Console.WriteLine(resultado);
                                EnviarMensaje(sender, resultado);
                                Console.WriteLine("ACTUALIZACIÓN APROBADA ORDENANTE");
                            }
                            break;
                        }
                    }

                }
                else if (partesOperación[0] == "R")
                {
                    //Console.WriteLine("EMPIEZA A ACTUALZIAR BENEFICIARIO");
                    string[] Actualizar = partesOperación[1].Split(";");
                    string beneficiario = Actualizar[0];
                    float dineroDepositado = float.Parse(Actualizar[1]);

                    foreach (string[] dato in datos)
                    {
                        if (dato[0] == beneficiario)
                        {
                            //Console.WriteLine("Encontró beneficiario ");
                            float dinero = float.Parse(dato[1]);
                            float dineroActualizado = dinero + dineroDepositado;

                            dato[1] = dineroActualizado.ToString();
                            Escribir(dato[0] + ";" + dato[1], int.Parse(dato[0]));
                            Console.WriteLine("TERMINÓ DE ESCRIBIR EN EL CSV");
                            string resultado = "S--R-" + beneficiario + "-" + (dineroActualizado.ToString()) + "-" + partesOperación[2];
                            Console.WriteLine(resultado);
                            EnviarMensaje(sender, resultado);
                            //enviar a replica

                            //EnviarMensaje(repli, texto);
                            Console.WriteLine("FIN DE ACTUALIZACIÓN BENEFICIARIO");
                            break;
                        }
                    }


                }

            }

        }

        static void LeerCSV(List<string[]>lista,int mi,int ma)
        {
            // Abre el archivo CSV
            using (StreamReader reader = new StreamReader("bd.csv"))
            {
                // Lee el resto del archivoddddd
                int cont = 0;
                while (!reader.EndOfStream)
                {
                    
                    string line = reader.ReadLine();
                    string[] valores = line.Split(';');

                    // Muestra los valores
                    if (cont >= limite[0] && cont <= limite[1]) {
                        //Console.WriteLine("Valores: " + valores[0] + ", " + valores[1]);
                        lista.Add(new string[] { valores[0], valores[1] });
                    }
                    cont++;
                }
                reader.Close();
            }
        }

        static void Escribir(string s,int fila) {

            try
            {
            /*
            // Abre el archivo CSV en modo lectura
            string valoresFila = s;
            // Lista temporal para almacenar las líneas del archivo CSV
            var lineas = new List<string>();
            // Abre el archivo CSV en modo lectura
            using (var reader = new StreamReader("bd.csv"))
            {
                // Recorre cada línea del archivo CSV
                while (true){   
                    string linea = reader.ReadLine();
                    if (linea == null)
                    {
                        // Fin del archivo CSV
                        break;
                    }
                    // Si es la fila que deseas sobrescribir, escribe los valores modificados
                    if (lineas.Count == fila)
                    {
                        linea = valoresFila;
                    }
                    // Añade la línea a la lista temporal
                    lineas.Add(linea);
                }
            }
            // Abre el archivo CSV en modo escritura
            using (var writer = new StreamWriter("bd.csv"))
            {
                // Recorre la lista temporal y escribe cada línea en el archivo CSV
                foreach (string linea in lineas)
                {
                    writer.WriteLine(linea);
                }
            }*/
            // Cadena con los valores de la fila
            string valoresFila =s;

            // Abre el archivo CSV en modo lectura y escritura
            using (var stream = new FileStream("bd.csv", FileMode.Open, FileAccess.ReadWrite))
            {
                using (var writer = new StreamWriter(stream))
                {
                    // Mueve el puntero al principio de la fila que deseas sobrescribir
                    stream.Seek((fila-1) * (valoresFila.Length + 2), SeekOrigin.Begin);

                    // Sobrescribe la fila///////////
                    writer.Write(valoresFila);
                }
            }
            }
            catch (Exception e)
            {

                Console.WriteLine(e.ToString());
            }

        }

    }
}
