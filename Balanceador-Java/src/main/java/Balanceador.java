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

        // print the array to check the data
        for (int i = 0; i < row; i++) {
            for (int j = 0; j < 2; j++) {
                //System.out.print(data[i][j] + " ");
            }
            System.out.println();
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
        int datalength = data.length;
        int segmentlength = segment.size();
        int balance = datalength / segmentlength;
        mTcpServer.sendBalancingTCPServer(balance, segment, datalength);
        System.out.println(balance);
    }
    void ServidorRecibe(String llego) throws InterruptedException {
        System.out.println("SERVIDOR40 El mensaje:" + llego);
        String[] t =llego.split("tt");
        if(t[0].equals("c")){
            ProducerRecibe(t[1]);
        }else{
            if (llego.equals("Client")) {
                AddClient(mTcpServer.IDClient());
                System.out.println("SERVIDOR40 El mensaje:" + llego);
            }
            else if (llego.equals("Segment"))
                AddSegment(mTcpServer.IDClient());
                System.out.println("SERVIDOR40 El mensaje:" + llego);
            /*else if (!llego.equals("listo") && blockingQueue.remainingCapacity()!=0)
                blockingQueue.put(llego);
            if (blockingQueue.remainingCapacity()==0) {
                String cola = "COLA LLENA ESPERA POR FAVOR";
                System.out.println(cola);
                mTcpServer.sendProducerMessageTCPServer(cola, client.get(0));
            }
            if (llego.equals("listo"))
                ready = true;
            System.out.println("SERVIDOR40 El mensaje:" + llego);
            if ((ready==true && segment.size()>0 && client.size()>0) && !blockingQueue.isEmpty()) {
                ready = false;
                //System.out.println(blockingQueue.remainingCapacity());
                ServidorEnvia("s");
            }*/

        }



    }

    void ProducerRecibe(String llego) throws InterruptedException {
       if (!llego.equals("listo") && consumingQueue.remainingCapacity()!=0)
           consumingQueue.put(llego);
        if (consumingQueue.remainingCapacity()==0) {
            String cola = "COLA LLENA ESPERA POR FAVOR";
            System.out.println(cola);
            mTcpServer.sendProducerMessageTCPServer(cola, segment.get(0));
        }
        if (llego.equals("listo"))
            readyOther = true;
        System.out.println("SERVIDOR40 El mensaje:" + llego);
        if ((readyOther==true && segment.size()>0 && client.size()>0) && !consumingQueue.isEmpty()) {
            readyOther = false;
            //System.out.println(consumingQueue.remainingCapacity());
            ConsumingEnvia("s");
        }
    }
    void ServidorEnvia(String sus) throws InterruptedException {
        String envia = blockingQueue.take();
        if (mTcpServer != null) {
            mTcpServer.sendConsumingMessageTCPServer(envia, segment,-1);
        }
    }
    void ConsumingEnvia(String sus) throws InterruptedException {
        String enviaP = consumingQueue.take();
        if (mTcpServer != null) {
            mTcpServer.sendConsumingMessageTCPServer(enviaP, client,-1);
        }
    }
    void AddClient(int ID) {
        client.add(ID);
    }

    void AddSegment(int ID) {
        segment.add(ID);
    }
}
