def decimal_a_tiempo(valor):
    decimal = (valor * 24.0) / 360.0
    horas = int(decimal)
    minutos = int((decimal - horas) * 60)
    segundos = (((decimal - horas) * 60 - minutos) * 60.0)

    tiempo_formateado = "({:02d}, {:02d}, {:02f})".format(horas, minutos, segundos)
    return tiempo_formateado

# Ejemplo de uso
numero_decimal = 101.28799  # Puedes cambiar este valor según tus necesidades
tiempo_resultante = decimal_a_tiempo(numero_decimal)
print(f"El número decimal {numero_decimal} es equivalente a {tiempo_resultante}")