var l_citys = [];
var city = null;
var idConstellationSelect = null;
var l_constellations = [];

window.addEventListener("load", (event) => {

    /*var bodyElement = document.body;
    bodyElement.onload = () => {
       
       pageMainLoad();
      };*/
    pageMainLoad();


});
function pageMainLoad() {
    document.getElementById("spinner").style.display = "none";

    var pagina = getNamePage();
    //|| pagina == 'index.html'

    if (pagina == 'estrellas.html') {

        loadIndex();

    } else if (pagina == 'estrellas_v2.html') {
        // loadStarsStellarium();

    } else if (pagina == 'config.html') {
        loadConfig();

    }
    else if (pagina == 'constelaciones.html') {
        loadConstelaciones();

    }
    else if (pagina == 'servos.html') {
        loadServos();

    } else if (pagina == 'servos_v2.html') {
        actulizarGradosServos_v2();
    } else if (pagina == 'espaciolab.html') {
        // loadIndex();
    } else if (pagina == 'config_v2.html') {
        htmlGetConfig();
    }
    if (pagina == 'estrellas.html' || pagina == 'config.html' || pagina == 'constelaciones.html') {
        fetchGetCity().then(el_city => {
            city = el_city;
        }).then(c => {

            if (document.getElementById("divMsgHead") !== null && city != null) {
                document.getElementById("divMsgHead").innerHTML = 'Lugar: ' + city.name;
            }
            if (document.getElementById("miCombo") !== null) {
                let element = document.getElementById("miCombo");
                element.value = city.id;
            }
        });
    }
}

function getNamePage() {
    var url = location.href;
    var pagina = url.substring(url.lastIndexOf('/') + 1);
    return pagina;
}

function onchangeCity() {
    //result.textContent = `You like ${event.target.value}`;
    var city_aux = l_citys.find(checkValue);
    if (city_aux != null) {
        fetchSetCity(city_aux.id, city_aux.name, String(city_aux.latitude), String(city_aux.longitude)).then(x => {
            city = x;
        });
    }
};
function checkValue(cmb) {
    return cmb.id == document.getElementById("miCombo").value;
}

function loadConfig() {
    fetchGetCitys().then(el_city => {
        var strHtml = '';
        l_citys = el_city;
        if (l_citys != null && l_citys.length > 0) {
            /*if (city == null) {
                city = l_citys[0];
            }*/
            const select = document.querySelector('select');
            l_citys.forEach(element => {
                strHtml += '<option  class="list-group-item" value="' + element.id + '">' + element.name + '</option >';
            }
            );
            if (document.getElementById("miCombo") !== null) {
                document.getElementById("miCombo").innerHTML = strHtml;
            }
        }

    })
}
function loadIndex() {
    //$("#spinner").hide();
    //document.getElementById("spinner").style.display = "none";
    fetchStarsJSON().then(stars => {
        var strHtml = '';
        //stars; // fetched movies

        stars.forEach(element => {
            if (element.name != null && element.name != '') {//&& element.visible
                var disabled = '';
                if (!element.visible) {
                    disabled = ' disabled list-group-item-dark ';
                }
                var cssColor = '';
                if (element.nearZenith) {
                    cssColor = ' li-nearZenith ';
                }
                strHtml += '<li class="list-group-item' + disabled + cssColor + '" value="' + element.id + '">' + element.name + '</li>';
            }
        }
        );
        // document.getElementById("datalistOptionsStar").innerHTML = strHtml;
        document.getElementById("miLista").innerHTML = strHtml;

        // Obtén una referencia a la lista de elementos <li>
        var elementosLi = document.querySelectorAll("#miLista li");

        // Asigna un controlador de eventos a cada elemento <li>
        elementosLi.forEach(function (li) {
            li.addEventListener("click", function (event) {
                capturarEvento(li);
            });
        });
    });

}
function loadStarsStellarium() {

    fetchStarsStellariumJSON().then(stars => {
        var strHtml = '';
        stars.forEach(element => {
            if (element.name != null && element.name != '') {
                var disabled = '';
                if (!element.visible) {
                    disabled = ' disabled list-group-item-dark ';
                }
                var cssColor = '';
                if (element.nearZenith) {
                    cssColor = ' li-nearZenith ';
                }
                strHtml += '<li class="list-group-item' + disabled + cssColor + '" value="' + element.id + '">' + element.name + '</li>';
            }
        }
        );
        document.getElementById("miLista").innerHTML = strHtml;
        var elementosLi = document.querySelectorAll("#miLista li");
        elementosLi.forEach(function (li) {
            li.addEventListener("click", function (event) {
                capturarEvento(li);
            });
        });
    });

}
function loadConstelaciones() {
    //$("#spinner").hide();
    //document.getElementById("spinner").style.display = "none";
    fetchConstellationsJSON().then(l => {
        var strHtml = '';
        //stars; // fetched movies
        l_constellations = l;
        l.forEach(element => {
            if (element.name != null && element.name != '' && element.visible) {
                var disabled = '';
                /*if (!element.visible) {
                    disabled = ' disabled list-group-item-dark ';
                }*/
                strHtml += '<li class="list-group-item' + disabled + '" value="' + element.id + '">' + element.name + ' (' + element.nameLatin + ')' + '</li>';
            }
        }
        );
        // document.getElementById("datalistOptionsStar").innerHTML = strHtml;
        document.getElementById("miLista").innerHTML = strHtml;

        // Obtén una referencia a la lista de elementos <li>
        var elementosLi = document.querySelectorAll("#miLista li");

        // Asigna un controlador de eventos a cada elemento <li>
        elementosLi.forEach(function (li) {
            li.addEventListener("click", function (event) {
                capturarEvento_ConstellationsConstellations(li);
            });
        });
    });
}
function loadServos() {
    actulizarGradosServos();
    var rangeH = document.getElementById("rangeHorizontal");
    var bubbleH = document.getElementById("bubbleHorizontal");
    rangeH.addEventListener("input", () => {
        setBubble(rangeH, bubbleH);
    });
    var rangeV = document.getElementById("rangeVertical");
    var bubbleV = document.getElementById("bubbleVertical");
    rangeV.addEventListener("input", () => {
        setBubble(rangeV, bubbleV);
    });
}
//
function onchangeRange(newVal) {
    // document.getElementById("valBox").innerHTML=newVal;
    // actulizarGradosServos();

    onchangeRangeMoverServo();
}

function setBubble(range, bubble) {
    const val = range.value;
    const min = range.min ? range.min : 0;
    const max = range.max ? range.max : 100;
    const newVal = Number(((val - min) * 100) / (max - min));
    bubble.innerHTML = val;

    // Sorta magic numbers based on size of the native UI thumb
    // bubble.style.left = newVal = "%";
}
function onClickVolver() {
    //window.location.href = "index.html";
    // location.reload();
    history.back();
    //
    /*setTimeout(function() {

        location.reload();
    }, 5000);*/
    return false;
}
function onClickIrConfig() {
    window.location.href = "config.html";
    return false;
}
function onClickIrEstrellas() {
    window.location.href = "estrellas.html";
    return false;
}
function onClickIrEstrellasv2() {
    window.location.href = "estrellas_v2.html";
    return false;
}
function onClickIrConstelaciones() {
    window.location.href = "constelaciones.html";
    return false;
}
function onClickIrServos() {
    window.location.href = "servos.html";
    return false;
}
function onClickIrEspacioLab() {
    window.location.href = "espaciolab.html";
    return false;
}
function onClickIrVersion_v1() {
    window.location.href = "index_v1.html";
    return false;
}
function onClickIrConfiguracion() {
    window.location.href = "config_v2.html";
    return false;
}
function onClickIrServos_v2() {
    window.location.href = "servos_v2.html";
    return false;
}
function quitarActiveLi() {

    var elementosLi = document.querySelectorAll("#miLista li");

    // Asigna un controlador de eventos a cada elemento <li>
    elementosLi.forEach(function (li) {
        li.classList.remove("active");
    });
}
// Función para capturar el evento
function capturarEvento(elemento) {
    quitarActiveLi();
    elemento.classList.add("active");
    onClickStar(elemento.value);
    //  alert("Se hizo clic en el elemento: " + elemento.textContent + " id: " + elemento.value);
    // Aquí puedes realizar las acciones que desees al capturar el evento
}
function capturarEvento_ConstellationsConstellations(elemento) {
    quitarActiveLi();
    elemento.classList.add("active");
    onClickConstellation(elemento.value);
}

function onClickConstellation(pId) {
    if (!isOnClickStar) {
        isOnClickStar = true;
        // $("#spinner").show();
        document.getElementById("spinner").style.display = '';
        document.getElementById("cardDetalle").style.display = '';
        document.getElementById("selectStar").value = 0;
        document.getElementById("inputName").value = '';
        var id = pId;
        idConstellationSelect = pId;
        cargarDetalleConstellation();
        fetchServoConstellation(id).then(text => {
            var strHtml = '';
            strHtml += ' <div class="alert alert-primary" role="alert">' + text + '  </div>';
            document.getElementById("divMsg").innerHTML = strHtml;
            isOnClickStar = false;
            document.getElementById("spinner").style.display = "none";// $("#spinner").hide();
        });
    }
}
function cargarDetalleConstellation() {
    //idConstellationSelect

    var arrayConst = l_constellations.filter(f => f.id == idConstellationSelect);
    var constelacion = arrayConst[0];
    document.getElementById("inputName").value = constelacion.name;
    if (constelacion.idHD_startRef != 0) {
        document.getElementById("selectStar").value = constelacion.idHD_startRef;
    }

}

function onClickGuardarConstellation() {
    // idConstellationSelect
    var id = idConstellationSelect;
    var idHD = document.getElementById("selectStar").value;
    var oName = document.getElementById("inputName").value;
    fetchUpdateConstelacion(id, idHD, oName);//.then(l => {
}
function actulizarGradosServos() {

    fetchGetServos().then(response => {
        var strHtml = '';
        /*const o = response.json();
        const json = '{"result":true, "count":42}';*/
        const o = JSON.parse(response);
        document.getElementById("inputServoH").value = o.horizontal;
        document.getElementById("inputServoV").value = o.vertical;
        //
        //document.getElementById("rangeHorizontal").value = o.horizontal;
        var rangeH = document.getElementById("rangeHorizontal");
        rangeH.value = o.horizontal;
        var bubbleH = document.getElementById("bubbleHorizontal");
        var rangeV = document.getElementById("rangeVertical");
        rangeV.value = o.vertical;
        var bubbleV = document.getElementById("bubbleVertical");

        setBubble(rangeH, bubbleH);
        setBubble(rangeV, bubbleV);
        //


        document.getElementById("inputServoHmin").value = o.horizontal_min;
        document.getElementById("inputServoHmax").value = o.horizontal_max;
        document.getElementById("inputServoVmin").value = o.vertical_min;
        document.getElementById("inputServoVmax").value = o.vertical_max;
        // document.getElementById("spinner").style.display = "none";// $("#spinner").hide();
    });
}
async function fetchStarsJSON() {
    const response = await fetch('/stars');
    const stars = await response.json();
    return stars;
}
async function fetchStarsStellariumJSON() {
    const response = await fetch('/stars_stellarium');
    const stars = await response.json();
    return stars;
}
async function fetchAllStarsJSON() {
    const response = await fetch('/allstars');
    const stars = await response.json();
    return stars;
}
async function fetchConstellationsJSON() {
    const response = await fetch('/constellations');
    const stars = await response.json();
    return stars;
}
async function fetchServo(pId) {
    const response = await fetch('/servo?id=' + pId);
    const text = await response.text();
    return text;
}
async function fetchServo_v2(pId) {
    const response = await fetch('/servo_v2?id=' + pId);
    const text = await response.json();
    return text;
}
function ajaxBegingCallFunction() {
    document.getElementById("spinner").style.display = '';
}
function ajaxEndCallFunction(pValue) {
    var strHtml = '';
    strHtml += ' <div class="alert alert-primary" role="alert">' + pValue + '  </div>';
    document.getElementById("divMsg").innerHTML = strHtml;
    document.getElementById("spinner").style.display = "none";
}
async function fetchServoConstellation(pId) {
    const response = await fetch('/servoconstellations?id=' + pId);
    const text = await response.text();
    return text;
}
function onClickApagarLaser() {
    ajaxBegingCallFunction();
    fetchLaser(0, 0).then(text => {
        ajaxEndCallFunction(text);
    });
}
function onClickEncenderLaser() {
    ajaxBegingCallFunction();
    fetchLaser(0, 1).then(text => {
        ajaxEndCallFunction(text);
    });
}
async function fetchLaser(pRead, pOn) {
    const response = await fetch('/laser?read=' + pRead + '&on=' + pOn);
    const text = await response.text();
    return text;
}

async function fetchGetCitys() {
    const response = await fetch('/citys');
    const text = await response.json();
    return text;
}
async function fetchGetCity() {
    const response = await fetch('/getcity');
    const text = await response.json();
    return text;
}
async function fetchSetCity(pId, pName, pLat, pLon) {// string name, double lat, double lon
    const response = await fetch('/setcity?id=' + pId + '&name=' + pName + '&lat=' + pLat + '&lon=' + pLon);
    const text = await response.json();
    return text;
}
async function fetchSetServoMover(pH, pV, pH_min, pH_max, pV_min, pV_max, pOnLaser) {
    const response = await fetch('/servomover?pH=' + pH + '&pV=' + pV + '&pH_min=' + pH_min + '&pH_max=' + pH_max + '&pV_min=' + pV_min + '&pV_max=' + pV_max + '&pOnLaser=' + pOnLaser);
    const text = await response.text();
    return text;
}
async function fetchSetServoMover_v2(pH, pV) {
    const response = await fetch('/servomover_v2?pH=' + pH + '&pV=' + pV);
    const text = await response.text();
    return text;
}
async function fetchGetServos() {
    const response = await fetch('/getservos');
    const text = await response.text();
    return text;
}
async function fetchGetServos_v2() {
    const response = await fetch('/getservos_v2');
    const text = await response.text();
    return text;
}
var isOnClickStar = false;

function onClickStar(pId) {
    if (!isOnClickStar) {
        isOnClickStar = true;
        // $("#spinner").show();
        document.getElementById("spinner").style.display = '';
        var id = pId;
        var pagina = getNamePage();
        //|| pagina == 'index.html'

        if (pagina == 'estrellas.html' || pagina == 'espaciolab.html') {

            fetchServo(id).then(text => {
                var strHtml = '';
                strHtml += ' <div class="alert alert-primary" role="alert">' + text + '  </div>';
                document.getElementById("divMsg").innerHTML = strHtml;
                isOnClickStar = false;
                document.getElementById("spinner").style.display = "none";// $("#spinner").hide();
            });

        } else { //if (pagina == 'estrellas_v2.html') 
            fetchServo_v2(id).then(oJson => {
                var text = '';
                var strEq = "AR/Dec: " + oJson.ec.ra + "/" + oJson.ec.dec;
                var strHc = "Az./Alt.: " + oJson.hc.Azimuth + "/" + oJson.hc.Altitude;
                var strSc = "H/V: " + oJson.sc.servoH + "/" + oJson.sc.servoV;
                text += strEq + "<br/>" + strHc + "<br/>" + strSc + "<br/>";
                text += "HIP " + oJson.hip + "<br/>";


                var strHtml = '';
                strHtml += ' <div class="alert alert-primary" role="alert">' + text + '  </div>';
                document.getElementById("divMsg").innerHTML = strHtml;
                isOnClickStar = false;
                document.getElementById("spinner").style.display = "none";// $("#spinner").hide();
            });

        }

    }
}
var isOnClickMoverServo = false;
function onClickMoverServo() {
    if (!isOnClickMoverServo) {
        isOnClickMoverServo = true;
        document.getElementById("spinner").style.display = '';
        var horizontal = 0;
        var vertical = 0;
        var horizontal_min = 0;
        var horizontal_max = 0;
        var vertical_min = 0;
        var vertical_max = 0;
        var laserOn = false;
        var inputElement_horizontal = document.getElementById('inputServoH');
        if (inputElement_horizontal.value.trim() !== '') {
            horizontal = inputElement_horizontal.value;
        }
        var inputElement_vertical = document.getElementById('inputServoV');
        if (inputElement_vertical.value.trim() !== '') {
            vertical = inputElement_vertical.value;
        }
        var inputElement_horizontal_min = document.getElementById('inputServoHmin');
        if (inputElement_horizontal_min.value.trim() !== '') {
            horizontal_min = inputElement_horizontal_min.value;
        }
        var inputElement_horizontal_max = document.getElementById('inputServoHmax');
        if (inputElement_horizontal_max.value.trim() !== '') {
            horizontal_max = inputElement_horizontal_max.value;
        }
        var inputElement_vertical_min = document.getElementById('inputServoVmin');
        if (inputElement_vertical_min.value.trim() !== '') {
            vertical_min = inputElement_vertical_min.value;
        }
        var inputElement_vertical_max = document.getElementById('inputServoVmax');
        if (inputElement_vertical_max.value.trim() !== '') {
            vertical_max = inputElement_vertical_max.value;
        }

        var checkboxLaserOn = document.getElementById('checkboxLaserOn');

        if (checkboxLaserOn.checked) {
            laserOn = true;
        }
        fetchSetServoMover(horizontal, vertical, horizontal_min, horizontal_max, vertical_min, vertical_max, laserOn).then(text => {
            actulizarGradosServos();
            var strHtml = '';
            strHtml += ' <div class="alert alert-primary" role="alert">' + text + '  </div>';
            document.getElementById("divMsg").innerHTML = strHtml;
            isOnClickMoverServo = false;
            document.getElementById("spinner").style.display = "none";

        })
    }
    return false;
}
function onchangeRangeMoverServo() {
    if (!isOnClickMoverServo) {
        isOnClickMoverServo = true;
        document.getElementById("spinner").style.display = '';
        var horizontal = 0;
        var vertical = 0;
        var horizontal_min = 0;
        var horizontal_max = 0;
        var vertical_min = 0;
        var vertical_max = 0;
        var laserOn = false;
        var inputElement_horizontal = document.getElementById('rangeHorizontal');
        if (inputElement_horizontal.value.trim() !== '') {
            horizontal = inputElement_horizontal.value;
        }
        var inputElement_vertical = document.getElementById('rangeVertical');
        if (inputElement_vertical.value.trim() !== '') {
            vertical = inputElement_vertical.value;
        }


        var inputElement_horizontal_min = document.getElementById('inputServoHmin');
        if (inputElement_horizontal_min.value.trim() !== '') {
            horizontal_min = inputElement_horizontal_min.value;
        }
        var inputElement_horizontal_max = document.getElementById('inputServoHmax');
        if (inputElement_horizontal_max.value.trim() !== '') {
            horizontal_max = inputElement_horizontal_max.value;
        }
        var inputElement_vertical_min = document.getElementById('inputServoVmin');
        if (inputElement_vertical_min.value.trim() !== '') {
            vertical_min = inputElement_vertical_min.value;
        }
        var inputElement_vertical_max = document.getElementById('inputServoVmax');
        if (inputElement_vertical_max.value.trim() !== '') {
            vertical_max = inputElement_vertical_max.value;
        }

        //var checkboxLaserOn = document.getElementById('checkboxLaserOn');

        //if (checkboxLaserOn.checked) {
        // laserOn = true;
        // }
        fetchSetServoMover(horizontal, vertical, horizontal_min, horizontal_max, vertical_min, vertical_max, laserOn).then(text => {
            actulizarGradosServos();
            var strHtml = '';
            strHtml += ' <div class="alert alert-primary" role="alert">' + text + '  </div>';
            document.getElementById("divMsg").innerHTML = strHtml;
            isOnClickMoverServo = false;
            document.getElementById("spinner").style.display = "none";

        })
    }
    return false;
}
/////////////////
var isOnClickMoverServo_v2 = false;
function onClickMoverServo_v2() {
    if (!isOnClickMoverServo_v2) {
        isOnClickMoverServo_v2 = true;
        document.getElementById("spinner").style.display = '';
        var horizontal = 0;
        var vertical = 0;

        var inputElement_horizontal = document.getElementById('inputServoH');
        if (inputElement_horizontal.value.trim() !== '') {
            horizontal = inputElement_horizontal.value;
        }
        var inputElement_vertical = document.getElementById('inputServoV');
        if (inputElement_vertical.value.trim() !== '') {
            vertical = inputElement_vertical.value;
        }

        fetchSetServoMover_v2(horizontal, vertical).then(text => {
            actulizarGradosServos_v2();
            var strHtml = '';
            strHtml += ' <div class="alert alert-primary" role="alert">' + text + '  </div>';
            document.getElementById("divMsg").innerHTML = strHtml;
            isOnClickMoverServo_v2 = false;
            document.getElementById("spinner").style.display = "none";
        })
    }
    return false;
}
function actulizarGradosServos_v2() {

    fetchGetServos_v2().then(response => {
        var strHtml = '';
        /*const o = response.json();
        const json = '{"result":true, "count":42}';*/
        const o = JSON.parse(response);
        document.getElementById("inputServoH").value = o.horizontal;
        document.getElementById("inputServoV").value = o.vertical;
        //
        //document.getElementById("rangeHorizontal").value = o.horizontal;
        var rangeH = document.getElementById("rangeHorizontal");
        rangeH.value = o.horizontal;
        var bubbleH = document.getElementById("bubbleHorizontal");
        var rangeV = document.getElementById("rangeVertical");
        rangeV.value = o.vertical;
        var bubbleV = document.getElementById("bubbleVertical");

        setBubble(rangeH, bubbleH);
        setBubble(rangeV, bubbleV);
        //


        // document.getElementById("inputServoHmin").value = o.horizontal_min;
        // document.getElementById("inputServoHmax").value = o.horizontal_max;
        // document.getElementById("inputServoVmin").value = o.vertical_min;
        // document.getElementById("inputServoVmax").value = o.vertical_max;
        // document.getElementById("spinner").style.display = "none";// $("#spinner").hide();
    });
}
function onchangeRangeMoverServo_v2() {
    if (!isOnClickMoverServo_v2) {
        isOnClickMoverServo_v2 = true;
        document.getElementById("spinner").style.display = '';
        var horizontal = 0;
        var vertical = 0;

        var inputElement_horizontal = document.getElementById('rangeHorizontal');
        if (inputElement_horizontal.value.trim() !== '') {
            horizontal = inputElement_horizontal.value;
        }
        var inputElement_vertical = document.getElementById('rangeVertical');
        if (inputElement_vertical.value.trim() !== '') {
            vertical = inputElement_vertical.value;
        }




        fetchSetServoMover_v2(horizontal, vertical).then(text => {
            actulizarGradosServos_v2();
            var strHtml = '';
            strHtml += ' <div class="alert alert-primary" role="alert">' + text + '  </div>';
            document.getElementById("divMsg").innerHTML = strHtml;
            isOnClickMoverServo_v2 = false;
            document.getElementById("spinner").style.display = "none";

        });
    }
    return false;
}
function onClickGrabarConfig() {

    document.getElementById("spinner").style.display = '';

    var latitude = document.getElementById("txt_latitude").value;
    var longitude = document.getElementById("txt_longitude").value;
    var horizontal_grados_min = document.getElementById("txt_horizontal_grados_min").value;
    var horizontal_grados_max = document.getElementById("txt_horizontal_grados_max").value;
    var vertical_grados_min = document.getElementById("txt_vertical_grados_min").value;
    var vertical_grados_max = document.getElementById("txt_vertical_grados_max").value;
    var horizontal_grados_calibrate = document.getElementById("txt_horizontal_grados_calibrate").value;
    var vertical_grados_calibrate = document.getElementById("txt_vertical_grados_calibrate").value;

    setConfig(latitude, longitude, horizontal_grados_min, horizontal_grados_max, vertical_grados_min, vertical_grados_max, horizontal_grados_calibrate, vertical_grados_calibrate).then(text => {
        var strHtml = '';
        strHtml += ' <div class="alert alert-primary" role="alert">' + text + '  </div>';
        document.getElementById("divMsg").innerHTML = strHtml;
        document.getElementById("spinner").style.display = "none";

    })


}
function htmlGetConfig() {
    fetchGetConfig().then(response => {
        var strHtml = '';

        const o = JSON.parse(response);
        document.getElementById("txt_latitude").value = o.latitude;
        document.getElementById("txt_longitude").value = o.longitude;
        document.getElementById("txt_horizontal_grados_min").value = o.horizontal_grados_min;
        document.getElementById("txt_horizontal_grados_max").value = o.horizontal_grados_max;
        document.getElementById("txt_vertical_grados_min").value = o.vertical_grados_min;
        document.getElementById("txt_vertical_grados_max").value = o.vertical_grados_max;
        document.getElementById("txt_horizontal_grados_calibrate").value = o.horizontal_grados_calibrate;
        document.getElementById("txt_vertical_grados_calibrate").value = o.vertical_grados_calibrate;
        //

    });
    // txt_latitude
    // txt_longitude
    // txt_horizontal_grados_min
    // txt_horizontal_grados_max
    // txt_vertical_grados_min
    // txt_vertical_grados_max
}
async function fetchGetConfig() {
    const response = await fetch('/getConfig');
    const text = await response.text();
    return text;
}
async function setConfig(latitude, longitude, horizontal_grados_min, horizontal_grados_max, vertical_grados_min, vertical_grados_max, horizontal_grados_calibrate, vertical_grados_calibrate) {
    const response = await fetch('/setConfig?latitude=' + latitude + '&longitude=' + longitude + '&horizontal_grados_min=' + horizontal_grados_min + '&horizontal_grados_max=' + horizontal_grados_max
        + '&vertical_grados_min=' + vertical_grados_min + '&vertical_grados_max=' + vertical_grados_max + '&horizontal_grados_calibrate=' + horizontal_grados_calibrate + '&vertical_grados_calibrate=' + vertical_grados_calibrate);
    const text = await response.text();
    return text;
}