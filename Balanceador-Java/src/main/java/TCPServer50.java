import java.io.BufferedReader;
import java.io.PrintWriter;
import java.net.ServerSocket;
import java.net.Socket;
import java.util.ArrayList;
import java.util.concurrent.TimeUnit;

public class
TCPServer50 {
    private String message;
    int nrcli = 0;
    public static final int SERVERPORT = 4444;
    private OnMessageReceived messageListener = null;
    private boolean running = false;
    TCPServerThread50[] sendclis = new TCPServerThread50[10];
    PrintWriter mOut;
    BufferedReader in;
    ServerSocket serverSocket;
    //el constructor pide una interface OnMessageReceived
    public TCPServer50(OnMessageReceived messageListener) {
        this.messageListener = messageListener;
    }
    public OnMessageReceived getMessageListener(){
        return this.messageListener;
    }
    //Enviar mensaje de balanceo a todos los segmentos, repartiendo equitativamente
    public void sendBalancingTCPServer(int balance, ArrayList<Integer> segments,int datalength){
        for (int i = 0; i < segments.size(); i++) {
            if (i < segments.size() - 1) {
                message = "B-" + ((i * balance) + 1) + "-" + (i + 1) * balance;
                sendclis[segments.get(i)].sendMessage(message);
                System.out.println("ENVIANDO A SEGMENTO " + (i + 1));
            }
            if (i == segments.size() - 1) {
                message = "B-" + ((i * balance) + 1) + "-" + datalength;
                sendclis[segments.get(i)].sendMessage(message);
                System.out.println("ENVIANDO A SEGMENTO " + (i + 1));
            }
        }
    }
    //Enviar mensaje a cliente especifico
    public void sendClientMessageTCPServer(String message, int IDClient, int NClient){
        sendclis[IDClient].sendMessage(message);
        System.out.println("ENVIANDO A CLIENTE " + (NClient));
    }
    //Mensaje de consulta si es segmento o cliente
    public void sendProducerConsuming(int index) {
        sendclis[index].sendMessage("Is Segment or Client?");
        System.out.println("ENVIANDO A CONEXION " + (index));
    }
    //Enviar mensaje a segmento especifico
    public void sendSegmentMessageTCPServer(String message, int indexSegment, int NSegment){
        sendclis[indexSegment].sendMessage(message);
        System.out.println("ENVIANDO A SEGMENTO " + (NSegment));
    }

    public int IDClient(){
        return nrcli;
    }
    
    public void run(){
        running = true;
        try{
            System.out.println("TCP Server"+"S : Connecting...");
            serverSocket = new ServerSocket(SERVERPORT);
            
            while(running){
                Socket client = serverSocket.accept();
                System.out.println("TCP Server"+"S: Receiving...");
                nrcli++;
                System.out.println("Engendrado " + nrcli);
                sendclis[nrcli] = new TCPServerThread50(client,this,nrcli,sendclis);
                Thread t = new Thread(sendclis[nrcli]);
                t.start();
                System.out.println("Nuevo conectado:"+ nrcli+" jugadores conectados");
                //Sleep de verificacion
                TimeUnit.SECONDS.sleep(1);
                sendProducerConsuming(nrcli);
            }
            
        }catch( Exception e){
            System.out.println("Error"+e.getMessage());
        }finally{

        }
    }
    public  TCPServerThread50[] getClients(){
        return sendclis;
    } 

    public interface OnMessageReceived {
        public void messageReceived(String message) throws InterruptedException;
    }
}
