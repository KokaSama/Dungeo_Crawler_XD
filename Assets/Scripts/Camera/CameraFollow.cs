using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Header("Seguimiento")]
    [SerializeField] private Transform objetivo;
    [Tooltip("Posición de la cámara respecto al objetivo. (-15, 15, -15) = vista isométrica desde el suroeste.")]
    [SerializeField] private Vector3 desplazamiento = new Vector3(-15f, 15f, -15f);
    [Tooltip("Tiempo aproximado (s) que tarda la cámara en alcanzar al objetivo. 0 = pegada al jugador.")]
    [SerializeField, Min(0f)] private float suavizado = 0.15f;

    [Header("Zoom (rueda del ratón)")]
    [SerializeField] private float zoomInicial = 9f;
    [SerializeField] private float zoomMinimo = 5f;
    [SerializeField] private float zoomMaximo = 16f;
    [Tooltip("Cuánto cambia el tamaño ortográfico por cada 'click' de la rueda.")]
    [SerializeField] private float pasoZoom = 1.5f;
    [Tooltip("Rapidez con la que el zoom llega al valor deseado.")]
    [SerializeField] private float suavizadoZoom = 10f;

    private Camera camara;
    private Vector3 velocidadActual;
    private float tamanoObjetivo;

    private void Awake()
    {
        camara = GetComponent<Camera>();
        camara.orthographic = true;
        tamanoObjetivo = Mathf.Clamp(zoomInicial, zoomMinimo, zoomMaximo);
        camara.orthographicSize = tamanoObjetivo;
    }

    private void Start()
    {
       
        if (objetivo != null)
            transform.position = objetivo.position + desplazamiento;
    }

    private void LateUpdate()
    {
        if (objetivo == null)
            return;

        Vector3 destino = objetivo.position + desplazamiento;
        transform.position = Vector3.SmoothDamp(transform.position, destino, ref velocidadActual, suavizado);

        ActualizarZoom();
    }

    private void ActualizarZoom()
    {
        if (Mouse.current != null)
        {
            float rueda = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(rueda) > 0.01f)
                tamanoObjetivo = Mathf.Clamp(tamanoObjetivo - Mathf.Sign(rueda) * pasoZoom, zoomMinimo, zoomMaximo);
        }

        
        float t = 1f - Mathf.Exp(-suavizadoZoom * Time.deltaTime);
        camara.orthographicSize = Mathf.Lerp(camara.orthographicSize, tamanoObjetivo, t);
    }
}
