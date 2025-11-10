#if SUBNAUTICA
using System.Linq;
#endif
using System.Text;
using Nautilus.Extensions;
using UnityEngine;

namespace Nautilus.GUI;

/// <summary>
/// Standardized methods for rendering game input bindings (i.e., buttons and keys) as formatted strings in the UI.
/// </summary>
public static class InputDisplayStrings
{
    /// <summary>
    /// Hex representation of the standard color used in both Subnautica and Below Zero when
    /// rendering an input binding. All methods in this class default to this color if no other
    /// color code is specified.
    /// </summary>
    public static readonly string DefaultInputDisplayColor = "#ADF8FFFF";

    /// <summary>
    /// Returns a glyph representing the button or key identified by the specified
    /// <see cref="KeyCode"/>.
    /// </summary>
    /// <param name="boundKeyCode">The key code to display.</param>
    /// <param name="colorHex">The color in which to render the glyph, as a hexadecimal string,
    /// optionally including a leading "#". If not provided, a default color will be used.</param>
    /// <returns>A string containing a key or button glyph and styling tags.</returns>
    public static string DisplayStringForInputBinding(KeyCode boundKeyCode, string colorHex = null)
    {
        return DisplayStringForInputBinding(boundKeyCode.KeyCodeToString(), colorHex);
    }

    /// <summary>
    /// Returns a glyph representing the button, key, or axis identified by the given name.
    /// </summary>
    /// <param name="binding">The name of the button or key to display, e.g.,
    /// <c>"MouseButtonLeft"</c>. </param>
    /// <param name="colorHex">The color in which to render the glyph, as a hexadecimal string,
    /// optionally including a leading "#". If not provided, a default color will be used.</param>
    /// <returns>A string containing the glyph and styling tags.</returns>
    public static string DisplayStringForInputBinding(string binding, string colorHex = null)
    {
#if SUBNAUTICA
        return GameInput.GetDisplayText(binding, NormalizeHexColor(colorHex));
#elif BELOWZERO
        return $"<color={NormalizeHexColor(colorHex)}>{uGUI.GetDisplayTextForBinding(binding)}</color>";
#endif
    }

    /// <summary>
    /// Renders one or more glyphs, styled with color, representing one or more bindings for the
    /// specified game input.
    /// </summary>
    /// <param name="input">The game input/action to display.</param>
    /// <param name="colorHex">The color in which to render the glyph(s), as a hexadecimal string,
    /// optionally including a leading "#". If not provided, a default color will be used.</param>
    /// <param name="allBindingSets">If <c>true</c>, render all buttons/keys bound to the action;
    /// otherwise, render only the first (default) button or key for the action.</param>
    /// <param name="bindingSeparator">String to separate glyphs if the action has multiple
    /// bindings.</param>
    /// <param name="gamePadOnly">If <c>true</c>, render only gamepad buttons/axes bound to the
    /// action, ignoring bindings defined on other input devices; otherwise, render bindings for the
    /// current input device.</param>
    /// <returns>A string containing the glyph or glyphs, and styling tags.</returns>
    public static string DisplayStringForInput(
        GameInput.Button input,
        string colorHex = null,
        bool allBindingSets = false,
        string bindingSeparator = " / ",
        bool gamePadOnly = false)
    {
        colorHex = NormalizeHexColor(colorHex);
        StringBuilder sb = Pool<StringBuilderPool>.Get().sb;

#if SUBNAUTICA
        GameInput.BindingSet[] bindingSets =
            allBindingSets ? GameInput.AllBindingSets : [.. GameInput.AllBindingSets.Take(1)];
        GameInput.Device device =
            gamePadOnly ? GameInput.Device.Controller : GameInput.PrimaryDevice;

        string[] bindings =
            [.. bindingSets.Select(set => GameInput.GetBinding(device, input, set))
                           .Where(binding => binding != null)];
        if (bindings.Length < 1)
        {
            GameInput.AppendColor(colorHex, Language.main.Get("NoInputAssigned"), sb);
            return sb.ToString();
        }

        GameInput.AppendDisplayText(bindings[0], sb, colorHex);
        foreach (string binding in bindings.Skip(1))
        {
            sb.Append(bindingSeparator);
            GameInput.AppendDisplayText(binding, sb, colorHex);
        }
#elif BELOWZERO
        uGUI.AppendBindings(sb, input, allBindingSets, bindingSeparator, gamePadOnly);
        if (colorHex != DefaultInputDisplayColor)
        {
            // If a specific color is desired, replace the hard-coded value.
            sb.Replace(DefaultInputDisplayColor, $"{colorHex}");
        }
#endif

        return sb.ToString();
    }

    private static string NormalizeHexColor(string hexColor)
    {
        if (hexColor == null) { return DefaultInputDisplayColor; }

        return hexColor.StartsWith("#") ? hexColor : "#" + hexColor;
    }
}
