# me - this DAT.
# webServerDAT - the connected Web Server DAT
# request - A dictionary of the request fields. The dictionary will always contain the below entries, plus any additional entries dependent on the contents of the request
# 		'method' - The HTTP method of the request (ie. 'GET', 'PUT').
# 		'uri' - The client's requested URI path. If there are parameters in the URI then they will be located under the 'pars' key in the request dictionary.
#		'pars' - The query parameters.
# 		'clientAddress' - The client's address.
# 		'serverAddress' - The server's address.
# 		'data' - The data of the HTTP request.
# response - A dictionary defining the response, to be filled in during the request method. Additional fields not specified below can be added (eg. response['content-type'] = 'application/json').
# 		'statusCode' - A valid HTTP status code integer (ie. 200, 401, 404). Default is 404.
# 		'statusReason' - The reason for the above status code being returned (ie. 'Not Found.').
# 		'data' - The data to send back to the client. If displaying a web-page, any HTML would be put here.
import os
import sys

# Ensure Pillow is available
try:
    from PIL import Image
except ImportError:
    # Add paths to Python libraries
    # Modify these paths based on your TouchDesigner Python environment
    python_paths = [
        'C:/Program Files/Derivative/TouchDesigner/bin/Lib/site-packages',
        os.path.expanduser('~/.local/lib/python3.x/site-packages')
    ]
    
    for path in python_paths:
        if os.path.exists(path):
            sys.path.append(path)
    
import json
from PIL import Image
import numpy
import io

# return the response dictionary
def onHTTPRequest(webServerDAT, request, response):
	# get the uri from the request header
	uri = request['uri']

	# if the root is requested send back the initial website
	if uri == '/':
		response['statusCode'] = 200 # OK
		response['statusReason'] = 'OK'
		response['data'] = op('index').text
	# if this is looking for something in the libs folder
	# check if the dat exists and if so, send back the content
	elif request['uri'] == '/random':
	
		img = op('null_img')
		
		# convert from float-32 to 8-bit RGBA
		# for speed reasons we are fetching
		# a delayed version, so changes are
		# usualy only visible on the next reload
		# of the page
		rgbA = img.numpyArray(delayed=True)*255
		rgbA = numpy.flipud(rgbA)
		rgbA = rgbA[:,:,:-1] #strip off alpha
		rgbA = rgbA.astype(numpy.uint8)
			
		im = Image.fromarray(rgbA, mode='RGB')
		
		# save into memory buffer
		fio = io.BytesIO()
		im.save(fio, 'JPEG')
		
		# rewind and grab data
		fio.seek(0)
		d = fio.read()
		response['data'] = d
			
		# prepare next image
		op('noise1').par.seed += 1
		op('noise2').par.seed += 1
	elif uri.startswith('/libs/'):
		response['statusCode'] = 200 # OK
		response['statusReason'] = 'OK'
		if op(uri[1:]):
			response['data'] = op(uri[1:]).text
	# else just respond with 200/OK
	else:
		response['statusCode'] = 200 # OK
		response['statusReason'] = 'OK'
	
	return response

def onWebSocketOpen(webServerDAT, client):
	# when a client connects, add it to the timer
	# with the default update frequency of 10 frames
	print('opening')
	op('table_clients').appendRow([client,10])
	op('timer_update').par.start.pulse()
	return

def onWebSocketClose(webServerDAT, client):
	# when a client disconnects, remove it from the timer
	tbl = op('table_clients')
	if tbl.row(client):
		tbl.deleteRow(client)
	return

def onWebSocketReceiveText(webServerDAT, client, data):
	# if the client sends data back to the server
	# convert the json to a dict and look for
	# the updateFreq value. Finally update the frequency
	# for the client in the segment table of the timer
	# if the key error is in the dict, then update the 
	# error column which determines if errors will be send
	# to the client
	dataDict = json.loads(data)
	print(dataDict)
	updateFreq = dataDict.get('updateFreq',None)
	type = dataDict.get('type',None)
	error = dataDict.get('error',None)
	if updateFreq:
		tbl = op('table_clients')
		if tbl.row(client):
			tbl[client,'length'] = updateFreq
	if type == "imageSelection":
		imageID = dataDict.get('imageId',None)
		op('slider1').par.value0 = (int(imageID)/4)
	if type == "speedSelection":
		speedID = dataDict.get('speedId',None)
		op('update_speed').par.value0 = (float(speedID)/100)
	if error != None:
		tbl = op('table_clients')
		if tbl.row(client):
			tbl[client,'error'] = int(error)
	return

def onWebSocketReceiveBinary(webServerDAT, client, data):
	webServerDAT.webSocketSendBinary(client, data)
	return

def onServerStart(webServerDAT):
	# clean up clients table
	op('table_clients').clear(keepFirstRow=True)
	op('timer_update').par.initialize.pulse()
	return

def onServerStop(webServerDAT):
	# clean up clients table
	op('table_clients').clear(keepFirstRow=True)
	op('timer_update').par.initialize.pulse()
	return