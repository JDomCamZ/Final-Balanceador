import socket
import sys
import threading
import time
import random

ip = '192.168.1.37'
port = 4444
mensaje = ""
id_cliente = 1
mi_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
mi_socket.connect((ip, port))


def recibir():
    while (True):
        if mensaje == "adios\n":
            break
        respuesta = mi_socket.recv(4000).decode()
        print("se recibio: " + respuesta)


def working():
    timetosleep = random.random() * 3 + 1
    time.sleep(timetosleep)
    mensaje = 'listo\n'
    mi_socket.send(mensaje.encode())
    print("termino el trabajo")


def send(mensaje):
    mi_socket.send(mensaje.encode())
    if mensaje == "adios\n":
        mi_socket.close()
        sys.exit()


# C--01-L-672
def lectura():
    mensaje = "C--" + "0" + str(id_cliente) + "-L-" + str(random.randint(1, 1000))
    print(mensaje)
    send(mensaje)


# C--02-A-420;730;30.20
def actualizar():
    mensaje = "C--" + "0" + str(id_cliente) + "-A-" + str(random.randint(1, 1000))
    mensaje = mensaje + ";" + str(random.randint(1, 1000))+";" + str(round(random.random()*1000, 2))
    print(mensaje)
    send(mensaje)


mensaje = 'listo\n'
send(mensaje)
respuesta = mi_socket.recv(4000).decode()

if (respuesta == "Is Segment or Client?\r\n"):
    mensaje = 'Client\n'
    send(mensaje)
    print("Esperando mensaje del balanceador")

    respuesta = mi_socket.recv(4000).decode()
    splits = respuesta.split()

    if (splits[0] == 'Cliente'):
        id_cliente = splits[1]
        print("el numero de cliente es: " + id_cliente)
    threadRecib = threading.Thread(name="h1", target=recibir, args=())
    threadRecib.start()

lectura()
actualizar()

