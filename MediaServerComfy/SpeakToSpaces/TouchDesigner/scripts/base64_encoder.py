import base64



def onPerformAction(scriptOp):
	
	
    # Get the TOP you want to convert
    top = op('photo_'+str(int(op('math5')['v1'])))
    # Use .save() method with a full path
    import os
    import tempfile
    
    # Get a temp file path
    temp_dir = tempfile.gettempdir()
    image_path = os.path.join(temp_dir, 'td_export.jpg')
    
    # Save the TOP as an image
    top.save(image_path)
    
    # Read the image and base64 encode
    with open(image_path, 'rb') as image_file:
        base64_image = base64.b64encode(image_file.read()).decode('utf-8')
    
    # Create a JSON message
    import json
    message = json.dumps({
    	'imageId':int(op('math5')['v1']),
        'imageData': base64_image
    })
    # Output to the Text DAT
    scriptOp.text = message
    #ws.send(message)