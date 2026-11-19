using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DocenteTopicItemUI : MonoBehaviour
{
    public Toggle toggleSeleccion;
    public TMP_Text nombreTemaText;
    public TMP_InputField inputCantidad;
    public Button btnMenos;
    public Button btnMas;

    [HideInInspector] public string temaNombre;

    public void Configurar(string tema)
    {
        this.temaNombre = tema;

        if (nombreTemaText != null)
        {
            nombreTemaText.text = tema;
        }

        if (inputCantidad != null)
        {
            inputCantidad.text = "1";
            inputCantidad.onValueChanged.AddListener(OnCantidadInputChanged);
        }

        if (toggleSeleccion != null)
        {
            toggleSeleccion.isOn = false;
        }

        if (btnMenos != null)
        {
            btnMenos.onClick.RemoveAllListeners();
            btnMenos.onClick.AddListener(() => ModificarCantidad(-1));
        }

        if (btnMas != null)
        {
            btnMas.onClick.RemoveAllListeners();
            btnMas.onClick.AddListener(() => ModificarCantidad(1));
        }
    }

    private void ModificarCantidad(int delta)
    {
        int cant = ObtenerCantidad();
        cant = Mathf.Clamp(cant + delta, 1, 99);
        if (inputCantidad != null)
        {
            inputCantidad.text = cant.ToString();
        }
    }

    private void OnCantidadInputChanged(string val)
    {
        if (int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out int num))
        {
            if (num < 1) inputCantidad.text = "1";
            else if (num > 99) inputCantidad.text = "99";
        }
    }

    public bool EstaSeleccionado()
    {
        return toggleSeleccion != null && toggleSeleccion.isOn;
    }

    public int ObtenerCantidad()
    {
        if (inputCantidad == null) return 1;
        if (int.TryParse(inputCantidad.text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int c))
        {
            return Mathf.Max(1, c);
        }
        return 1;
    }
}
