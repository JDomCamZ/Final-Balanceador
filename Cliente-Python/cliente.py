import socket
import sys
import threading
import time
import random

ip = '192.168.1.37'
port = 4444

mi_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
mi_socket.connect((ip, port))
mensaje = ""


def recibir():
    while (True):
        if mensaje == "adios\n":
            break
        respuesta = mi_socket.recv(4000).decode()
        print("se recibio: " + respuesta)
        working()


def working():
    timetosleep = random.random() * 3 + 1
    time.sleep(timetosleep)
    mensaje = 'listo\n'
    mi_socket.send(mensaje.encode())
    print("termino el trabajo")


def send():
    while True:
        escribe = input()
        mensaje = "ctt" + escribe
        mensaje += '\n'
        mi_socket.send(mensaje.encode())
        if mensaje == "cppadios\n":
            mi_socket.close()
            sys.exit()


def conect(ip_, port_):
    mi_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    mi_socket.connect((ip_, port_))
    mensaje = 'listo\n'
    mi_socket.send(mensaje.encode())
    respuesta = mi_socket.recv(4000).decode()

    if (respuesta == "Is Segment or Client?\r\n"):
        mensaje = 'Client\n'
        mi_socket.send(mensaje.encode())
    print("Esperando mensaje de Segment")

    threadRecib = threading.Thread(name="h1", target=recibir, args=())
    threadSend = threading.Thread(name="h2", target=send, args=())

    threadRecib.start()
    threadSend.start()


mensaje = 'listo\n'
mi_socket.send(mensaje.encode())
respuesta = mi_socket.recv(4000).decode()

if (respuesta == "Is Segment or Client?\r\n"):
    mensaje = 'Client\n'
    mi_socket.send(mensaje.encode())
print("Esperando mensaje de Segment")

threadRecib = threading.Thread(name="h1", target=recibir, args=())
threadSend = threading.Thread(name="h2", target=send, args=())

threadRecib.start()
threadSend.start()
