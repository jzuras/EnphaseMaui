# GUI Design Specification

## Overview
This document defines the design specification for the main monitoring interface used by both the MAUI Blazor Hybrid app and the Blazor WASM PWA, based on the Enphase Envoy hardware display but enhanced for modern GUI capabilities.

## Layout Structure

### Top Section - Current Values
**Two-column layout** displaying real-time energy metrics:

**Left Column - Producing**
- Large numerical display (e.g., "0.00 kW")
- Label: "Producing"
- Shows current solar panel energy production

**Right Column - Consuming**  
- Large numerical display (e.g., "3.83 kW")
- Label: "Consuming"
- Shows current household energy consumption

### Center Section - Net Energy Flow
**Prominent display** showing the net energy balance:
- Large numerical display with sign indicator (e.g., "-3.83 kW")
- Positive values: producing more than consuming (surplus)
- Negative values: consuming more than producing (deficit)
- This should be the most visually prominent element

### Bottom Section - Update Controls
**Four-button layout** for refresh interval control:

**Button Layout:**
- **Now** - Triggers immediate data refresh (doesn't change auto-refresh interval)
- **15** - Sets 15-second auto-refresh interval
- **30** - Sets 30-second auto-refresh interval  
- **60** - Sets 60-second auto-refresh interval

**Button Behavior:**
- Active interval button should have visual indication (highlight/different color)
- "Now" button provides immediate feedback but doesn't stay active
- Only one interval button active at a time

## Design Principles

### Visual Hierarchy
1. **Primary**: Net energy flow (center, largest)
2. **Secondary**: Current producing/consuming values (top, medium)
3. **Tertiary**: Update controls (bottom, smaller)

### Modern UI Enhancements
- **Color Scheme**: Modern palette (not limited to black/white hardware constraints)
- **Typography**: Clean, readable fonts (not limited to segment display)
- **Responsive Design**: Adapts to different screen sizes and orientations
- **Smooth Transitions**: Animated updates for data changes
- **Visual Feedback**: Clear indication of active states and user interactions

### Platform Considerations
- **Cross-Platform**: Design works on Windows, Android, iOS, and web browsers
- **PWA Support**: Interface works as installable Progressive Web App
- **Touch-Friendly**: Button sizes appropriate for mobile interaction
- **Accessibility**: Proper contrast ratios and text sizing
- **Performance**: Efficient updates without excessive redraws
- **Ultra-Responsive**: Scales from desktop monitors down to 60px width windows
- **Dynamic Sizing**: Real-time responsiveness when resizing MAUI or browser windows

## Data Display Format
- **Energy Values**: Display in kW with 2 decimal places
- **Positive/Negative**: Clear visual indication of energy flow direction
- **Units**: Consistent "kW" labeling throughout interface
- **Real-Time**: Values update based on selected refresh interval

## User Experience Flow
1. User launches app and sees current energy status immediately
2. Net energy flow is instantly visible (most important metric)
3. User can see breakdown of producing vs consuming
4. User can adjust refresh rate based on monitoring needs
5. "Now" button available for immediate updates when needed

## Technical Implementation Notes
- Data sourced from local `envoy.local` APIs
- MAUI app: Direct API access with SSL bypass
- PWA app: API access via CORS proxy service
- 15-second default refresh interval (matching existing TUI behavior)
- Graceful handling of API errors or network issues
- Efficient data caching to minimize API calls
- State persistence for user's preferred refresh interval

## Responsive Design Features ✅ IMPLEMENTED

### Ultra-Responsive Scaling
- **12 Breakpoints**: 1024px, 768px, 480px, 320px, 280px, 240px, 200px, 160px, 120px, 100px, 80px, 60px
- **Font Scaling**: From 3.5rem net display down to 0.4rem at smallest sizes
- **Progressive Layout**: Grid columns adapt from 4→2→1 based on screen width
- **Micro Optimizations**: Zero padding and 0.15rem fonts at extreme sizes
- **Cross-Platform**: Identical scaling behavior in MAUI apps and browsers

### Debug Information Management ✅ IMPLEMENTED
- **Error-Context Display**: Debug info only appears when errors occur
- **Clean Success States**: No debug clutter during normal operation
- **Technical Details**: Service info, token status, API response details when needed

### Interface Streamlining ✅ IMPLEMENTED
- **Two-Page Design**: Monitor (default) and Current Token pages only
- **No Menu Bar**: Removed top "About" bar for maximum content space
- **Direct Access**: Application launches directly into monitoring dashboard

## Future Enhancements
- Historical data visualization (graphs/charts)
- Alert notifications for significant changes
- Additional metrics (daily/monthly totals)
- Export functionality for data analysis
- Customizable display themes