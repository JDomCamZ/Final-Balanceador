import java.io.File;
import java.util.ArrayList;
import java.util.Scanner;
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.LinkedBlockingDeque;
import java.io.BufferedReader;
import java.io.FileReader;
import java.io.IOException;

public class Balanceador {
    TCPServer50 mTcpServer;
    Scanner sc;
    BlockingQueue<String> blockingQueue = new LinkedBlockingDeque<>(5);
    BlockingQueue<String> consumingQueue = new LinkedBlockingDeque<>(5);
    ArrayList<Integer> client = new ArrayList<Integer>();
    ArrayList<Integer> segment = new ArrayList<Integer>();
    static int[][] data = new int[1000][2];
    boolean ready = false;
    boolean readyOther=false;
    int datalength;
    int segmentlength;
    int balance;
    public static void main(String[] args) throws InterruptedException {
        File file = new File("Balanceador-Java", "src");
        file = new File(file, "main");
        file = new File(file, "resources");
        file = new File(file, "bd.csv");
        String filePath = file.getAbsolutePath();
        System.out.println(filePath);
        String delimiter = ";";
        int row = 0;

        try (BufferedReader br = new BufferedReader(new FileReader(filePath))) {
            String line;
            while ((line = br.readLine()) != null) {
                String[] fields = line.split(delimiter);
                for (int col = 0; col < 2; col++) {
                    data[row][col] = Integer.parseInt(fields[col]);
                }
                row++;
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
    void Balanceo(){
        datalength = data.length;
        segmentlength = segment.size();
        balance = datalength / segmentlength;
        mTcpServer.sendBalancingTCPServer(balance, segment, datalength);
        System.out.println(balance);
    }
    void ServidorRecibe(String llego) throws InterruptedException {
        System.out.println("SERVIDOR40 El mensaje:" + llego);
        String[] t = llego.split("--");
        if(t[0].equals("C")){
            ClienteRecibe(t[1]);
        } else if(t[0].equals("S")){
            SegmentRecibe(t[1]);
        } else {
            if (llego.equals("Client")) {
                AddClient(mTcpServer.IDClient());
                String mess = "Cliente " + client.size();
                mTcpServer.sendClientMessageTCPServer(mess, client.get(client.size()-1), client.size());
                System.out.println("SERVIDOR40 El mensaje:" + llego);
            }
            else if (llego.equals("Segment"))
                AddSegment(mTcpServer.IDClient());
                System.out.println("SERVIDOR40 El mensaje:" + llego);
        }
    }
    void ClienteRecibe(String llego) throws InterruptedException {
        String[] t = llego.split("-");
        for (int i = 0; i < segmentlength; i++){
            if (i < segmentlength - 1) {
                if (t[1].equals("L")) {
                    if ((i * balance) + 1 <= Integer.parseInt(t[2]) && Integer.parseInt(t[2]) <= (i + 1) * balance) {
                        String message = t[1] + "-" + t[2]+ "-" + t[0];
                        mTcpServer.sendSegmentMessageTCPServer(message, segment.get(i), i + 1);
                    }
                }
                if (t[1].equals("A") || t[1].equals("R")) {
                    String[] s = t[2].split(";");
                    if ((i * balance) + 1 <= Integer.parseInt(s[0]) && Integer.parseInt(s[0]) <= (i + 1) * balance) {
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
        if (t[0].equals("L")) {
            String message = "El saldo de " + t[1] + " es: " + t[2];
            mTcpServer.sendClientMessageTCPServer(message, client.get(Integer.parseInt(t[3]) - 1), Integer.parseInt(t[3]));
        }
        if (t[0].equals("A")) {
            if (t[1].equals("A")){
                String message01 = "¡Transacción exitosa!\n" + "El nuevo saldo de " + t[2] + " es: " + t[3];
                String message02 = t[6] + "-R-" + t[4] + ";" + t[5];
                mTcpServer.sendClientMessageTCPServer(message01, client.get(Integer.parseInt(t[6]) - 1), Integer.parseInt(t[6]));
                ClienteRecibe(message02);
            }
            if (t[1].equals("D")){
                String message = "¡Transacción errónea!\n" + "El saldo de " + t[2] + " no es suficiente para realizar la transacción.";
                mTcpServer.sendClientMessageTCPServer(message, client.get(Integer.parseInt(t[3]) - 1), Integer.parseInt(t[3]));
            }
        }
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
