namespace nscore;

public static class Constantes
{
    public static string astro_type_star
    {
        get { return "star"; }
    }
    public static string astro_type_moon
    {
        get { return "moon"; }
    }
    public static string astro_type_servoAngle
    {
        get { return "servoAngle"; }
    }
    public static string astro_type_servoAngle_inicio
    {
        get { return "astro_type_servoAngle_inicio"; }
    }
    public static string astro_status_create
    {
        get { return "create"; }
    }
    public static string astro_status_calculationResolution
    {
        get { return "calculationResolution"; }
    }
    public static string astro_status_movingServo
    {
        get { return "movingServo"; }
    }
    public static string astro_status_movedServo
    {
        get { return "movedServo"; }
    }
    public static string astro_status_resetSession
    {
        get { return "resetSession"; }
    }
    public static double servo_sleep_max
    {
        get { return 3; }
    }
    public static double servo_sleep_min
    {
        get { return 0.5; }
    }
}
