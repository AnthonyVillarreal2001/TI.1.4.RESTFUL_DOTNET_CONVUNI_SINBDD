namespace _02.CLIMOV.Utils
{
    public static class ApiConfig
    {
        // ⚠️ CONFIGURACIÓN: Cambia estas constantes según tu escenario
        private const bool USE_EMULATOR = false;  // true = emulador Android, false = dispositivo físico
        private const string YOUR_PC_IP = "10.40.19.95";  // Tu IP de Windows (ipconfig)
        private const string PORT = "7118";

        // ⚠️ IMPORTANTE: Esta URL se ajusta automáticamente según la plataforma
        // El servidor REST debe estar corriendo en https://localhost:7118
        public static string BaseUrl
        {
            get
            {
#if ANDROID
                // En Android, "localhost" no funciona
                if (USE_EMULATOR)
                {
                    // Para emulador Android: usa 10.0.2.2 (dirección especial para localhost de Windows)
                    return $"https://10.0.2.2:{PORT}/api/conversion/convert";
                }
                else
                {
                    // Para dispositivo físico Android: usa la IP de tu PC
                    return $"https://{YOUR_PC_IP}:{PORT}/api/conversion/convert";
                }
#elif IOS
                // iOS Simulator puede usar localhost
                return $"https://localhost:{PORT}/api/conversion/convert";
#elif WINDOWS
                // Windows usa localhost
                return $"https://localhost:{PORT}/api/conversion/convert";
#elif MACCATALYST
                // MacCatalyst usa localhost
                return $"https://localhost:{PORT}/api/conversion/convert";
#else
                return $"https://localhost:{PORT}/api/conversion/convert";
#endif
            }
        }

        // Nota: Si sigues teniendo problemas de timeout:
        // 1. Verifica que el servidor esté corriendo (https://localhost:7118/swagger)
        // 2. Verifica el firewall de Windows
        // 3. Si usas dispositivo físico, asegúrate de estar en la misma red WiFi
        // 4. Considera usar HTTP en lugar de HTTPS para desarrollo (más fácil)
    }
}
