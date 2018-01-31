using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Camera))]
public class RtsSelectionSystem : MonoBehaviour {
    // mouse button to use
    public int mousebutton = 0;

    // only select objects that aren't behind walls?
    public bool checkVisibility = false;

    // selection rectangle
    Vector2 start = Vector2.zero;
    Vector2 cur = Vector2.zero;
    bool visible = false;

    // last selection
    List<GameObject> last = new List<GameObject>();

    // cache
    Camera cam;

    void Start() {
        cam = GetComponent<Camera>();
    }

    static Rect rect_around(Vector2 a, Vector2 b) {
        Vector2 min = new Vector2(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y));
        Vector2 max = new Vector2(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    public bool rect_contains3D(Camera cam, Rect r, Collider c) {
        // world to screen
        var s = cam.WorldToScreenPoint(c.bounds.center);

        // contains it?
        return r.Contains(new Vector2(s.x, Screen.height - s.y));
    }

    public bool is_visible(Camera cam, Collider c) {
        // world to screen position
        var s = cam.WorldToScreenPoint(c.bounds.center);

        // screen to world raycast to find out if anything is inbetween
        var ray = cam.ScreenPointToRay(s);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
            return c == hit.collider;
        return false;
    }

    // call 'OnSelect' for a GameObject
    static void call_onselect(GameObject g) {
        g.SendMessage("OnSelect", SendMessageOptions.DontRequireReceiver);
    }

    // call 'OnDeselect' for a GameObject
    static void call_ondeselect(GameObject g) {
        g.SendMessage("OnDeselect", SendMessageOptions.DontRequireReceiver);
    }

    // call 'OnSelect' for multiple GameObjects
    static void call_onselect_multi(List<GameObject> list) {
        foreach (var go in list)
            call_onselect(go);
    }

    // call ondeselect for each GameObject in 'list', unless its in 'ignore'
    static void call_ondeselect_multi_except(List<GameObject> list, List<GameObject> ignore) {
        foreach (var g in list)
            if (g != null && !ignore.Contains(g))
                call_ondeselect(g);
    }

    // find the clicked object by raycasting against colliders
    static GameObject find_clicked(Camera cam) {
        var ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
            return hit.transform.gameObject;
        return null;
    }

    List<GameObject> find_selected(Camera cam, Rect r) {
        // find all colliders in scene
        var all = FindObjectsOfType<Collider>().ToList();

        if (checkVisibility) {
            // find the ones that are in-rect & active & visible
            return (from c in all
                    where rect_contains3D(cam, r, c) && is_visible(cam, c) && c.gameObject.activeInHierarchy
                    select c.gameObject).ToList();
        } else {
            // find the ones that are in-rect & active
            return (from c in all
                    where rect_contains3D(cam, r, c) && c.gameObject.activeInHierarchy
                    select c.gameObject).ToList();
        }
    }

    void Update() {
        // multi selection started? set rect pos to current mouse pos
        if (Input.GetMouseButtonDown(mousebutton)) {
            start.x = Input.mousePosition.x;
            start.y = Screen.height - Input.mousePosition.y;
            visible = true;
        }

        // multi selection in progress? update rect
        if (Input.GetMouseButton(mousebutton)) {
            cur.x = Input.mousePosition.x;
            cur.y = Screen.height - Input.mousePosition.y;
        }

        // selection finished?
        if (Input.GetMouseButtonUp(mousebutton)) {
            // single click or multi selection?
            if (start == cur) {
                // simple raycasting then
                var go = find_clicked(cam);
                if (go != null) {
                    // deselect -> select
                    call_ondeselect_multi_except(last, new List<GameObject>{go});
                    call_onselect(go);

                    // save as last selection
                    last = new List<GameObject>{go};
                }
            } else {
                // find selected objects
                var selected = find_selected(cam, rect_around(start, cur));

                // deselect -> select
                call_ondeselect_multi_except(last, selected);
                call_onselect_multi(selected);

                // save as last selection
                last = selected;
            }

            // don't draw the rectangle anymore
            visible = false;
        }
    }

    void OnGUI() {
        if (visible && cur != start)
            GUI.Box(rect_around(start, cur), "");
    }
}
