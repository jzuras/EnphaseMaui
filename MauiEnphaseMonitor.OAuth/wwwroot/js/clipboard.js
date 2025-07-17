window.clipboardInterop = {
    copyToClipboard: async function (text) {
        try {
            if (navigator.clipboard && window.isSecureContext) {
                // Use modern Clipboard API if available and in secure context
                await navigator.clipboard.writeText(text);
                return true;
            } else {
                // Fallback method for older browsers or non-secure contexts
                const textArea = document.createElement('textarea');
                textArea.value = text;
                textArea.style.position = 'fixed';
                textArea.style.left = '-999999px';
                textArea.style.top = '-999999px';
                document.body.appendChild(textArea);
                textArea.focus();
                textArea.select();
                
                const result = document.execCommand('copy');
                document.body.removeChild(textArea);
                return result;
            }
        } catch (error) {
            console.error('Failed to copy to clipboard:', error);
            return false;
        }
    }
};