# me - this DAT.
# timerOp - the connected Timer CHOP
# cycle - the cycle index
# segment - the segment index
# fraction - the time in fractional form
#
# interrupt - True if the user initiated a premature
# interrupt, False if a result of a normal timeout.
#
# onInitialize(): if return value > 0, it will be
# called again after the returned number of frames.
import json

def onInitialize(timerOp):
	return 0

def onReady(timerOp):
	return
	
def onStart(timerOp):
	clients = op('webserver1').webSocketConnections
	#for client in clients:
		#sendStats(client)
	return
	
def onTimerPulse(timerOp, segment):
	return

def whileTimerActive(timerOp, segment, cycle, fraction):
	return

def onSegmentEnter(timerOp, segment, interrupt):
	return
	
def onSegmentExit(timerOp, segment, interrupt):
	return

def onCycleStart(timerOp, segment, cycle):
	return

def onCycleEndAlert(timerOp, segment, cycle, alertSegment, alertDone, interrupt):
	return
	
def onCycle(timerOp, segment, cycle):
	tbl = op('table_clients')
	if tbl.numRows > 1:
		client = tbl[segment+1,'client'].val
		#sendStats(client)
		sendImage(client)
	return

def onDone(timerOp, segment, interrupt):
	return

def getStats():
	valueOP = op('showCooks/values')
	data = {
			'type':'cook',
			'project':'{0} on TouchDesigner{1} build {2}'.format(project.name,app.version,app.build),
			'frametime': round(valueOP['msec'][0],4),
			'dropoutrate':int(valueOP['cook'][0]),
			'gpumem':round(valueOP['gpu_mem_used'][0],2),
			'cpumem':round(valueOP['cpu_mem_used'][0],2),
			'dropped':False
		}
	data = json.dumps(data)
	return data
	
def sendStats(client):
	data = getStats()
	serverOp = op('webserver1')
	clients = serverOp.webSocketConnections
	if client in clients:
		serverOp.webSocketSendText(client, data)

def sendImage(client):
	mod( op( 'base64_encoder' ) ).onPerformAction(op('scriptOP'))
	
	data = op('scriptOP').text
	#print('send image')
	serverOp = op('webserver1')
	clients = serverOp.webSocketConnections
	if client in clients:
		serverOp.webSocketSendText(client, data)