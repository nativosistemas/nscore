#include <WiFi.h>
#include <HTTPClient.h>
#include <Arduino_JSON.h>

/*Definimos nuestras credenciales de la red WiFi*/
const char* ssid = "Rx6";        // "Rx6"; --> Coronda: "HUAWEI-n3rB" --> Celu:  "motog24"
const char* pass = "f1515red20";  // "f1515red20"; --> Coronda: "h3RFy2VD" --> Celu:  "moto2030"
const byte gpio_led = 32;
//const byte gpio_servoH = 33;
//const byte gpio_servoV = 25;
const String device_publicID = "d0a157bf-0cf3-475c-b590-d246221590f0";
const String device_name = "esp32_stepper_laser";
const char* url = "https://estrellas.duckdns.org/";  //http://192.168.100.200   nuc: http://192.168.100.170  coronda: http://192.168.100.100/
String sessionDevice_publicID;
int delayMillisecond = 2;
byte pinArray[2][4] = {
  { 17, 16, 18, 19 },  //H
  { 15, 13, 12, 14 }   //V
};

int paso[8][4] = {
  { 1, 0, 0, 0 },
  { 1, 1, 0, 0 },
  { 0, 1, 0, 0 },
  { 0, 1, 1, 0 },
  { 0, 0, 1, 0 },
  { 0, 0, 1, 1 },
  { 0, 0, 0, 1 },
  { 1, 0, 0, 1 }
};
int paso_index_h = 0;
int paso_index_v = 0;
int paso_max = 8;
int paso_max_index = paso_max - 1;
void stepper(bool pDirection, int pIdStepper) {
  String msgPrint = "";
  int* pasoIndex;
  int pinIndex;
  if (pIdStepper == 0) {
    pasoIndex = &paso_index_h;
    pinIndex = 0;
    msgPrint = " h ";
  } else if (pIdStepper == 1) {
    pasoIndex = &paso_index_v;
    pinIndex = 1;
    msgPrint = " v ";
  }

  if (pDirection) {
    *pasoIndex = (*pasoIndex) + 1;
  } else {
    *pasoIndex = (*pasoIndex) - 1;
  }
  if ((*pasoIndex) < 0) {
    *pasoIndex = paso_max_index;
  }
  if ((*pasoIndex) > paso_max_index) {
    *pasoIndex = 0;
  }

  msgPrint += " N1: pin: " + String(pinArray[pinIndex][0]) + " (value: " + String(paso[(*pasoIndex)][0]) + ")";
  msgPrint += " N2: pin: " + String(pinArray[pinIndex][1]) + " (value: " + String(paso[(*pasoIndex)][1]) + ")";
  msgPrint += " N3: pin: " + String(pinArray[pinIndex][2]) + " (value: " + String(paso[(*pasoIndex)][2]) + ")";
  msgPrint += " N4: pin: " + String(pinArray[pinIndex][3]) + " (value: " + String(paso[(*pasoIndex)][3]) + ")";
  //Serial.println(msgPrint);
  digitalWrite(pinArray[pinIndex][0], paso[(*pasoIndex)][0]);  //N1
  digitalWrite(pinArray[pinIndex][1], paso[(*pasoIndex)][1]);  //N2
  digitalWrite(pinArray[pinIndex][2], paso[(*pasoIndex)][2]);  //N3
  digitalWrite(pinArray[pinIndex][3], paso[(*pasoIndex)][3]);  //N4
}
/*void moveStepper_ant(float pDegree, float pFactor, int pDelayMillisecond, int pIdStepper) {
  float pasos_float = (pDegree * 11.3777777778) * pFactor;
  int pasos = round(pasos_float);
  bool direction = true;
  if (pasos < 0) {
    direction = false;
  }
  Serial.println("grados: " + String(pDegree));
  Serial.println("pasos_float: " + String(pasos_float));
  Serial.println("pasos: " + String(pasos));
  int pasos_abs = abs(pasos);
  for (int x = 0; x < pasos_abs; x++) {
    Serial.println("paso: " + String(x) + " dirección: " + String(direction));
    stepper(direction, pIdStepper);
    delay(pDelayMillisecond);
  }
}*/
void moveStepper(float pDegree, float pDegree_old, float pFactor, int pDelayMillisecond, int pIdStepper) {
  if (pDegree_old != pDegree) {
    double for_old = pDegree_old;
    double for_new = pDegree;
    if (for_old == -9999.0) {
      for_old = 0;
    }
    Serial.println("for_old: " + String(for_old));
    Serial.println("for_new: " + String(for_new));
    if (for_old != for_new) {
      bool direction = true;
      if (for_new < for_old) {
        direction = false;
      }
      float degree = (for_new - for_old);
      float pasos_float = (degree * 11.3777777778) * pFactor;
      int pasos = round(pasos_float);
      int pasos_abs = abs(pasos);
      Serial.println("direction: " + String(direction));
      Serial.println("grados: " + String(degree));
      Serial.println("pasos_float: " + String(pasos_float));
      Serial.println("pasos: " + String(pasos));
      for (int x = 0; x < pasos_abs; x++) {
        //Serial.println("paso: " + String(x) + " dirección: " + String(direction));
        stepper(direction, pIdStepper);
        delay(pDelayMillisecond);
      }
    }
  }
}
void moveStepperH(float pDegree, float pDegree_old, int pDelayMillisecond) {
  float factorRuedaDentada = 4.8;
  moveStepper(pDegree, pDegree_old, factorRuedaDentada, pDelayMillisecond, 0);
}
void moveStepperV(float pDegree, float pDegree_old, int pDelayMillisecond) {
  float factorRuedaDentada = 4;
  moveStepper(pDegree, pDegree_old, factorRuedaDentada, pDelayMillisecond, 1);
}

// Función para realizar una solicitud HTTP GET y devolver el resultado
String httpGET(String url) {
  String resultado = "";  // Variable para almacenar el resultado de la solicitud

  // Inicializar el objeto HTTPClient
  HTTPClient http;

  // Comenzar la solicitud HTTP GET
  http.begin(url);
  //http.addHeader("Authorization: Basic" ,"cmljdmVhbDoxMjM0==");
  // Realizar la solicitud y obtener el código de respuesta
  int codigoDeRespuesta = http.GET();

  // Verificar el código de respuesta
  if (codigoDeRespuesta > 0) {
    Serial.println("Código HTTP ► " + String(codigoDeRespuesta));  //Print return code

    if (codigoDeRespuesta == 200) {
      resultado = http.getString();
    }
  } else {
    // Si hubo un error en la solicitud, imprimir el código de error
    resultado = "Error - Código de respuesta: " + String(codigoDeRespuesta);
  }

  // Finalizar la solicitud
  http.end();

  // Devolver el resultado de la solicitud
  return resultado;
}
String actionAnt_getAntTracking() {
  Serial.println("sessionDeviceAdd");
  String resultado = "";
  String url_sessionDeviceAdd = String(url) + "actionAnt_getAntTracking?pDevice_publicID=" + device_publicID + "&pSessionDevice_publicID=" + sessionDevice_publicID;  // pDevice_publicID, string pSessionDevice_publicID
  String result_get = httpGET(url_sessionDeviceAdd);
  Serial.println("result_get: " + result_get);
  if (result_get != "null") {
    const char* arrayCharRespuestaHTTP;
    arrayCharRespuestaHTTP = result_get.c_str();
    JSONVar myObject = JSON.parse(arrayCharRespuestaHTTP);
    bool isTiene_publicID = false;
    if (myObject.hasOwnProperty("sessionDevice_publicID_return")) {
      sessionDevice_publicID = String((const char*)myObject["sessionDevice_publicID_return"]);
    }
    //
    const char* publicID;
    if (myObject.hasOwnProperty("publicID")) {
      publicID = (const char*)myObject["publicID"];
      isTiene_publicID = true;
    }
    //
    if (isTiene_publicID && String(publicID) != "00000000-0000-0000-0000-000000000000") {
      const char* type;
      if (myObject.hasOwnProperty("type")) {
        type = (const char*)myObject["type"];
      }
      Serial.println("type ► " + String(type));
      if (String(type) == "laser") {
        int isLaser;
        if (myObject.hasOwnProperty("isLaser")) {
          isLaser = (int)myObject["isLaser"];
        }
        Serial.println("isLaser ► " + String(isLaser));
        if (isLaser == 1) {
          digitalWrite(gpio_led, HIGH);  // turn the LED on (HIGH is the voltage level)
        } else if (isLaser == 0) {
          digitalWrite(gpio_led, LOW);  // turn the LED off by making the voltage LOW
        }
      } else {
        digitalWrite(gpio_led, LOW);  // apagar laser antes de mover
        double horizontal_grados;
        if (myObject.hasOwnProperty("horizontal_grados")) {
          horizontal_grados = (double)myObject["horizontal_grados"];
        }
        double horizontal_grados_sleep;
        if (myObject.hasOwnProperty("horizontal_grados_sleep")) {
          horizontal_grados_sleep = (double)myObject["horizontal_grados_sleep"];
        }
        double horizontal_grados_ant = -9999.0;  // Un valor que no esperas que la variable tenga normalmente;
        if (myObject.hasOwnProperty("horizontal_grados_ant")) {
          horizontal_grados_ant = (double)myObject["horizontal_grados_ant"];
          if (isnan(horizontal_grados_ant)) {
            horizontal_grados_ant = -9999.0;
          }
        }
        double horizontal_grados_min;
        if (myObject.hasOwnProperty("horizontal_grados_min")) {
          horizontal_grados_min = (double)myObject["horizontal_grados_min"];
        }
        double horizontal_grados_max;
        if (myObject.hasOwnProperty("horizontal_grados_max")) {
          horizontal_grados_max = (double)myObject["horizontal_grados_max"];
        }
        Serial.println("horizontal_grados_sleep: " + String(horizontal_grados_sleep));
         Serial.println("inicio : H");
        if (horizontal_grados_ant != horizontal_grados) {
          //MOVER ENGRANAJE H
          moveStepperH(horizontal_grados, horizontal_grados_ant, delayMillisecond);
          //servoH.attach(gpio_servoH, horizontal_grados_min, horizontal_grados_max);
          //servoMove(servoH, horizontal_grados_ant, horizontal_grados);
        } else {
          Serial.println("horizontal_grados_ant == horizontal_grados");
        }
         Serial.println("fin : H");
        double vertical_grados;7458
        if (myObject.hasOwnProperty("vertical_grados")) {
          vertical_grados = (double)myObject["vertical_grados"];
        }
        double vertical_grados_sleep;
        if (myObject.hasOwnProperty("vertical_grados_sleep")) {
          vertical_grados_sleep = (double)myObject["vertical_grados_sleep"];
        }
        double vertical_grados_ant = -9999.0;  // Un valor que no esperas que la variable tenga normalmente;
        if (myObject.hasOwnProperty("vertical_grados_ant")) {
          vertical_grados_ant = (double)myObject["vertical_grados_ant"];
          if (isnan(vertical_grados_ant)) {
            vertical_grados_ant = -9999.0;
          }
        }
        double vertical_grados_min;
        if (myObject.hasOwnProperty("vertical_grados_min")) {
          vertical_grados_min = (double)myObject["vertical_grados_min"];
        }
        double vertical_grados_max;
        if (myObject.hasOwnProperty("vertical_grados_max")) {
          vertical_grados_max = (double)myObject["vertical_grados_max"];
        }
         Serial.println("inicio : V");
        if (vertical_grados_ant != vertical_grados) {
          //MOVER ENGRANAJE V
          moveStepperV(vertical_grados, vertical_grados_ant, delayMillisecond);
          //servoV.attach(gpio_servoV, vertical_grados_min, vertical_grados_max);
          //servoMove(servoV, vertical_grados_ant, vertical_grados);
        } else {
          Serial.println("vertical_grados_ant == vertical_grados");
        }
        Serial.println("fin : V");
        Serial.println("publicID: " + String(publicID));
        Serial.println("vertical_grados: " + String(horizontal_grados));
        Serial.println("vertical_grados: " + String(vertical_grados));
      }

      String url_close = String(url) + "esp32_setAstro?publicID=" + String(publicID) + "&pSessionDevice_publicID=" + sessionDevice_publicID;  //publicID
      Serial.println(url_close);
      String result_get = httpGET(url_close);

    } else {
      // no hay resultado de la funcion de la api
      Serial.println("no hay resultado de la funcion de la api");
      //servos_attach_detach();
    }
  }

  return resultado;
}

void setup() {
  pinMode(gpio_led, OUTPUT);
  //servoH.attach(gpio_servoH,500,2500);
  pinMode(pinArray[0][0], OUTPUT);  //IN1
  pinMode(pinArray[0][1], OUTPUT);  //IN2
  pinMode(pinArray[0][2], OUTPUT);  //IN3
  pinMode(pinArray[0][3], OUTPUT);  //IN4

  pinMode(pinArray[1][0], OUTPUT);  //IN1
  pinMode(pinArray[1][1], OUTPUT);  //IN2
  pinMode(pinArray[1][2], OUTPUT);  //IN3
  pinMode(pinArray[1][3], OUTPUT);  //IN4

  /*Iniciamos el terminal Serial para la escritura de algunos parámetros */
  Serial.begin(115200);
  /*Iniciamos la conexión a la red WiFi definida*/
  WiFi.begin(ssid, pass);
  delay(2000);
  /*En el terminal Serial, indicamos que se está realizando la conexión*/
  Serial.print("Se está conectando a la red WiFi denominada ");
  Serial.println(ssid);
  /*Mientras se realiza la conexión a la red, aparecerán puntos, hasta que se realice la conexión*/
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  /*Con la conexión ya realizada, se pasa a imprimir algunos datos importantes, como la dirección IP asignada a nuestro dispositivo*/
  Serial.println("");
  Serial.println("WiFi connected");
  Serial.println("IP address: ");
  Serial.println(WiFi.localIP());

  //
  //servos_attach_detach();
  sessionDevice_publicID = "00000000-0000-0000-0000-000000000000";
}

void loop() {
  if (WiFi.status() == WL_CONNECTED) {  //Check WiFi connection status
    //isSessionDeviceOk();

    Serial.println("sessionDevice_publicID ► " + sessionDevice_publicID);
    actionAnt_getAntTracking();

  } else {

    Serial.println("Error en la conexión WIFI");
  }

  delay(1000);
}
