import socket
import sys
import threading
import time
import random
import numpy as np

ip ='192.168.0.18' #'192.168.0.12'
port = 4444
id_cliente = 1
mi_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
mi_socket.connect((ip, port))


def recibir():
    while True:
        if mensaje == "adios\n":
            break
        respuesta = mi_socket.recv(4000).decode()
        print("se recibio: " + respuesta)


def send(mensaje):
    mi_socket.send(mensaje.encode())
    if mensaje == "adios\n":
        mi_socket.close()
        sys.exit()


# C--01-L-672
def lectura():
    mensaje = "C--" + "0" + str(id_cliente) + "-L-" + str(random.randint(1, 10000)) + "\n"
    print(mensaje)
    send(mensaje)


# C--02-A-420;730;30.20
def actualizar():
    mensaje = "C--" + "0" + str(id_cliente) + "-A-" + str(random.randint(1, 10000))
    mensaje = mensaje + ";" + str(random.randint(1, 10000))+";" + str(round(random.random()*1000, 2)) + "\n"
    print(mensaje)
    send(mensaje)


mensaje = 'listo\n'
send(mensaje)
respuesta = mi_socket.recv(4000).decode()

if respuesta == "Is Segment or Client?\r\n":
    mensaje = 'Client\n'
    send(mensaje)
    print("Esperando mensaje del balanceador")
    respuesta = mi_socket.recv(4000).decode()
    splits = respuesta.split()
    if splits[0] == 'Cliente':
        id_cliente = splits[1]
        print("el numero de cliente es: " + id_cliente)
    threadRecib = threading.Thread(name="h1", target=recibir, args=())
    threadRecib.start()

for i in range(100000):
    nRand = np.random.choice([0, 1], 100, p=[0.6, 0.4])
    index = random.randint(0, len(nRand) - 1)
    #nRand = random.choice(nRand)
    selected_element = nRand[index]
    if selected_element == 0:
        lectura()
    else:
        actualizar()
    time.sleep(1/10)

