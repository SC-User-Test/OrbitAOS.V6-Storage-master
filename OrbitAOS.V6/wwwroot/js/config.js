// Runtime Configuration for Containerized Environment
// This file provides environment-specific configuration that can be injected at runtime
// via Azure Key Vault CSI Driver and Kubernetes ConfigMaps

(function(window) {
    'use strict';
    
    // Configuration object that will be populated from environment variables
    // These values should be injected at container runtime via ConfigMaps/Secrets
    window.AppConfig = {
        // API Configuration - injected from ConfigMap
        apiBaseUrl: window.ENV_API_BASE_URL || '/api',
        apiTimeout: parseInt(window.ENV_API_TIMEOUT || '30000'),
        
        // Feature Flags - injected from ConfigMap
        enableDebugMode: (window.ENV_DEBUG_MODE || 'false') === 'true',
        enableAnalytics: (window.ENV_ENABLE_ANALYTICS || 'false') === 'true',
        
        // External Service URLs - injected from ConfigMap
        authServiceUrl: window.ENV_AUTH_SERVICE_URL || '/auth',
        
        // API Keys and Secrets - injected from Azure Key Vault via CSI Driver
        // These should NEVER be hardcoded and must come from mounted secrets
        apiKey: window.ENV_API_KEY || '',
        
        // Environment Information
        environment: window.ENV_NAME || 'development',
        version: window.ENV_APP_VERSION || '1.0.0'
    };
    
    // Freeze the configuration object to prevent runtime modifications
    Object.freeze(window.AppConfig);
    
    // Log configuration load (without sensitive data)
    if (window.AppConfig.enableDebugMode) {
        console.log('Application configuration loaded for environment:', window.AppConfig.environment);
    }
    
})(window);
