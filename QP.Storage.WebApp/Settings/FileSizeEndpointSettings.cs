﻿namespace QP.Storage.WebApp.Settings
{
    public class FileSizeEndpointSettings
    {
        /// <summary>
        /// Базовый путь до endpoint, возвращащей размер файлов
        /// </summary>
        public string BasePath { get; set; } = "/_filesize";
    }
}
