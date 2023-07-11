# Python program to 
# demonstrate creation of an
# assistant using wolf ram API
  
import wolframalpha
  
# Taking input from user
question = input('')
  
# App id obtained by the above steps
API_key = "G3UKYR-TA5GQWRVQ3"
app_id = wolframalpha.getfixture(API_key)
#client = Client(app_id)
# Instance of wolf ram alpha 
# client class
client = wolframalpha.Client(app_id)
#res = client.query('temperature in Washington, DC on October 3, 2012')  
# Stores the response from 
# wolf ram alpha
#res = client.query('StarData["Arcturus", {"RightAscension", "Declination"}]')
res = client.query(input='pi', assumption='*C.pi-_*NamedConstant-')
  
# Includes only text from the response
answer = next(res.results).text
  
print(answer)