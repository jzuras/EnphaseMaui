using Microsoft.AspNetCore.Components;
using MauiEnphaseMonitor.Shared.Services;

namespace MauiEnphaseMonitor.Shared.Pages;

public partial class CurrentToken
{
    [Inject]
    private ITokenStorage TokenStorage { get; set; } = null!;
    
    [Inject]
    private IClipboardService ClipboardService { get; set; } = null!;

    private string? CurrentTokenValue { get; set; }
    private string? TokenInput { get; set; }
    private bool IsLoading { get; set; } = false;
    private string? ErrorMessage { get; set; }
    private string? SuccessMessage { get; set; }
    private bool HasToken { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        await this.LoadTokenAsync();
    }

    private async Task LoadTokenAsync()
    {
        try
        {
            this.IsLoading = true;
            this.ErrorMessage = null;
            
            this.CurrentTokenValue = await this.TokenStorage.GetTokenAsync();
            this.HasToken = !string.IsNullOrEmpty(this.CurrentTokenValue);
        }
        catch (Exception ex)
        {
            this.ErrorMessage = $"Error loading token: {ex.Message}";
        }
        finally
        {
            this.IsLoading = false;
            this.StateHasChanged();
        }
    }

    private async Task SaveTokenAsync()
    {
        if (string.IsNullOrWhiteSpace(this.TokenInput))
        {
            this.ErrorMessage = "Please enter a token";
            return;
        }

        try
        {
            this.IsLoading = true;
            this.ErrorMessage = null;
            this.SuccessMessage = null;

            await this.TokenStorage.SetTokenAsync(this.TokenInput.Trim());
            this.SuccessMessage = "Token saved successfully!";
            this.TokenInput = "";
            
            // Reload to show the new token
            await this.LoadTokenAsync();
        }
        catch (Exception ex)
        {
            this.ErrorMessage = $"Error saving token: {ex.Message}";
        }
        finally
        {
            this.IsLoading = false;
            this.StateHasChanged();
        }
    }

    private async Task CopyTokenAsync()
    {
        if (string.IsNullOrEmpty(this.CurrentTokenValue))
        {
            this.ErrorMessage = "No token to copy";
            return;
        }

        try
        {
            this.IsLoading = true;
            this.ErrorMessage = null;
            this.SuccessMessage = null;

            await this.ClipboardService.SetTextAsync(this.CurrentTokenValue);
            this.SuccessMessage = "Token copied to clipboard successfully! You can now paste it into other apps.";
        }
        catch (Exception ex)
        {
            this.ErrorMessage = $"Error copying token to clipboard: {ex.Message}";
        }
        finally
        {
            this.IsLoading = false;
            this.StateHasChanged();
        }
    }

    private async Task ClearTokenAsync()
    {
        try
        {
            this.IsLoading = true;
            this.ErrorMessage = null;
            this.SuccessMessage = null;

            await this.TokenStorage.ClearTokenAsync();
            this.SuccessMessage = "Token cleared successfully!";
            
            // Reload to update display
            await this.LoadTokenAsync();
        }
        catch (Exception ex)
        {
            this.ErrorMessage = $"Error clearing token: {ex.Message}";
        }
        finally
        {
            this.IsLoading = false;
            this.StateHasChanged();
        }
    }

    private string GetMaskedToken()
    {
        if (string.IsNullOrEmpty(this.CurrentTokenValue))
        {
            return "No token stored";
        }

        if (this.CurrentTokenValue.Length <= 10)
        {
            return new string('*', this.CurrentTokenValue.Length);
        }

        // Show first 4 and last 4 characters
        return $"{this.CurrentTokenValue[..4]}...{this.CurrentTokenValue[^4..]}";
    }
}