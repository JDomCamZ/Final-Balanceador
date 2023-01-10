import java.io.File;
import java.util.ArrayList;
import java.util.Scanner;
import java.io.BufferedReader;
import java.io.FileReader;
import java.io.IOException;

public class Balanceador {
    TCPServer50 mTcpServer;
    Scanner sc;
    ArrayList<Integer> client = new ArrayList<Integer>();
    ArrayList<Integer> segment = new ArrayList<Integer>();
    static int[][] data = new int[10000][2];
    int datalength;
    int segmentlength;
    int balance;
    public static void main(String[] args) throws InterruptedException {
        //Leer datos de BBDD en .csv
        File file = new File("Segmento-C#", "Segmento");
        file = new File(file, "bin");
        file = new File(file, "Debug");
        file = new File(file, "netcoreapp3.1");
        file = new File(file, "bd.csv");
        /*File file = new File("Balanceador-Java", "src");
        file = new File(file, "main");
        file = new File(file, "resources");
        file = new File(file, "bd.csv");*/
        String filePath = file.getAbsolutePath();
        System.out.println(filePath);
        String delimiter = ";";
        int row = 0;
        int cont = 0;

        try (BufferedReader br = new BufferedReader(new FileReader(filePath))) {
            String line;
            while ((line = br.readLine()) != null) {
                if (cont > 0) {
                    String[] fields = line.split(delimiter);
                    for (int col = 0; col < 2; col++) {
                        if (col == 0) {
                            data[row][col] = Integer.parseInt(fields[col]);
                        }
                        if (col == 1) {
                            data[row][col] = (int) Float.parseFloat(fields[col]);
                        }
                    }
                    row++;
                }
                cont++;
            }
        } catch (IOException e) {
            e.printStackTrace();
        }

        Balanceador objser = new Balanceador();
        objser.iniciar();
    }
    void iniciar() throws InterruptedException {
        new Thread(
                new Runnable() {

                    @Override
                    public void run() {
                        mTcpServer = new TCPServer50(
                                new TCPServer50.OnMessageReceived(){
                                    @Override
                                    public void messageReceived(String message) throws InterruptedException {
                                            ServidorRecibe(message);
                                    }
                                }
                        );
                        mTcpServer.run();
                    }
                }
        ).start();
        //-----------------
        String salir = "n";
        sc = new Scanner(System.in);
        System.out.println("Servidor bandera 01");
        while( !salir.equals("s")){
            salir = sc.nextLine();
            if (salir.equals("balancear")) {
                 Balanceo();
            }
            //ServidorEnvia(salir);
        }
        System.out.println("Servidor bandera 02");
    }
    //Metodo para balancear datos
    void Balanceo(){
        datalength = data.length;
        segmentlength = segment.size();
        balance = datalength / segmentlength;
        mTcpServer.sendBalancingTCPServer(balance, segment, datalength);
        System.out.println(balance);
    }
    //Recibir mensajes general
    void ServidorRecibe(String llego) throws InterruptedException {
        System.out.println("SERVIDOR40 El mensaje:" + llego);
        String[] t = llego.split("--");
        //C-- Cliente S-- Segmento
        if(t[0].equals("C")){
            ClienteRecibe(t[1]);
        }
        //S--Operacion-*****-IDCliente
        else if(t[0].equals("S")){
            SegmentRecibe(t[1]);
        } else {
            //Caso se conecta cliente enviar mensaje de identificacion de cliente
            if (llego.equals("Client")) {
                AddClient(mTcpServer.IDClient());
                String mess = "Cliente " + client.size();
                mTcpServer.sendClientMessageTCPServer(mess, client.get(client.size()-1), client.size());
                System.out.println("SERVIDOR40 El mensaje:" + llego);
            }
            //Caso se conecta segmento se añade a la lista
            else if (llego.equals("Segment"))
                AddSegment(mTcpServer.IDClient());
                System.out.println("SERVIDOR40 El mensaje:" + llego);
        }
    }
    void ClienteRecibe(String llego) throws InterruptedException {
        //01-L-123 | ID Cliente - Operacion - ID a leer
        //02-A-42;250;20.30 | ID a reducir transaccion ; ID a aumentar transaccion ; monto de transaccion
        String[] t = llego.split("-");
        for (int i = 0; i < segmentlength; i++){
            if (i < segmentlength - 1) {
                if (t[1].equals("L")) {
                    if ((i * balance) + 1 <= Integer.parseInt(t[2]) && Integer.parseInt(t[2]) <= (i + 1) * balance) {
                        //Se envia al Segmento L-123-01 | Operacion - ID BBDD - ID Cliente
                        String message = t[1] + "-" + t[2]+ "-" + t[0];
                        mTcpServer.sendSegmentMessageTCPServer(message, segment.get(i), i + 1);
                    }
                }
                if (t[1].equals("A") || t[1].equals("R")) {
                    String[] s = t[2].split(";");
                    if ((i * balance) + 1 <= Integer.parseInt(s[0]) && Integer.parseInt(s[0]) <= (i + 1) * balance) {
                        //Se envia al Segmento A-22;45;60.30-02 | ID BBDD a decrecer ; ID BBDD a aumentar ; monto transaccion
                        //Se envia al Segmento R-45;60.30-02 | ID BBDD a aumentar ; monto transaccion
                        String message = t[1] + "-" + t[2]+ "-" + t[0];
                        mTcpServer.sendSegmentMessageTCPServer(message, segment.get(i), i + 1);
                    }
                }
            }
            if (i == segmentlength - 1) {
                if (t[1].equals("L")) {
                    if ((i * balance) + 1 <= Integer.parseInt(t[2]) && Integer.parseInt(t[2]) <= datalength) {
                        String message = t[1] + "-" + t[2] + "-" + t[0];
                        mTcpServer.sendSegmentMessageTCPServer(message, segment.get(i), i + 1);
                    }
                }
                if (t[1].equals("A") || t[1].equals("R")) {
                    String[] s = t[2].split(";");
                    if ((i * balance) + 1 <= Integer.parseInt(s[0]) && Integer.parseInt(s[0]) <= datalength) {
                        String message = t[1] + "-" + t[2] + "-" + t[0];
                        mTcpServer.sendSegmentMessageTCPServer(message, segment.get(i), i + 1);
                    }
                }
            }
        }
    }
    void SegmentRecibe(String llego) throws InterruptedException {
        String[] t = llego.split("-");
        //Segmento envia L-672-452.32-01 | Operacion - ID BBDD - Monto de ID - ID Cliente
        if (t[0].equals("L")) {
            String message = "El saldo de " + t[1] + " es: " + t[2];
            mTcpServer.sendClientMessageTCPServer(message, client.get(Integer.parseInt(t[3]) - 1), Integer.parseInt(t[3]));
        }
        if (t[0].equals("A")) {
            //Segmento envia A-A-420-470.30-730-30.20-02 | Operacion - Exito/Fracaso - ID BBDD actualizado - Monto nuevo - ID a actualizar - Monto transaccion- ID Cliente
            if (t[1].equals("A")){
                String message01 = "¡Transacción exitosa!\n" + "El nuevo saldo de " + t[2] + " es: " + t[3];
                String message02 = t[6] + "-R-" + t[4] + ";" + t[5];
                mTcpServer.sendClientMessageTCPServer(message01, client.get(Integer.parseInt(t[6]) - 1), Integer.parseInt(t[6]));
                ClienteRecibe(message02);
            }
            // Segmento envia A-D-420-02 | Operacion - Exito/Fracaso - ID BBDD - ID Cliente
            if (t[1].equals("D")){
                String message = "¡Transacción errónea!\n" + "El saldo de " + t[2] + " no es suficiente para realizar la transacción.";
                mTcpServer.sendClientMessageTCPServer(message, client.get(Integer.parseInt(t[3]) - 1), Integer.parseInt(t[3]));
            }
        }
        //Segmento envia R-730-80.40-02 | Operacion - ID BBDD actualizado - Monto actualizado - ID Cliente
        if (t[0].equals("R")) {
            String message = "El nuevo saldo de " + t[1] + " es: " + t[2];
            mTcpServer.sendClientMessageTCPServer(message, client.get(Integer.parseInt(t[3]) - 1), Integer.parseInt(t[3]));
        }
    }
    void AddClient(int ID) {
        client.add(ID);
    }

    void AddSegment(int ID) {
        segment.add(ID);
    }
}
