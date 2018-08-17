
using Evaluation.UnityInterface;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Evaluation.UnityInterface;

/// <summary>
/// Controller class to manage the field lines
/// </summary>
public class PendulumManager : MonoBehaviour
{
    [SerializeField]
    private GameObject PendulumWeight;
    [SerializeField]
    private GameObject StandRopeJoint;
    [SerializeField]
    private GameObject StopWatch;
    [SerializeField]
    private GameObject InfoTextPanel;
    
    public float ropeLength = 0.3f;
    public float weight = 1.0f;

    private  bool mouseDown;
    private Vector3 mouseStart;
    private GameObject lastLine;
    private Boolean slow = false;
    private Vector3 defaultPosition;


    private float weightChangeStepSize = 0.05f;
    private float weightMin = 1.0f;
    private float weightMax = 2.0f;

    private float ropeLengthChangeStepSize = 0.01f;
    private float ropeMin = 0.1f;
    private float ropeMax = 0.5f;

    
    /// <summary>
    /// Initialization
    /// </summary>
    public void Start()
    {

        defaultPosition = PendulumWeight.transform.position;

        //This is for initialy setting the ropelength in assessment 
        setRopeLengthRelative(0);

        Calculator.OnButtonPressed += CalculatorButtonPressed;
    }

    public void Update()
    {

        HingeJoint joint = PendulumWeight.GetComponent<HingeJoint>();

        if (Input.GetMouseButtonUp(0) && mouseDown)
        {
            Debug.Log("Sending Release action");
            IterationResult res = AssessmentManager.Instance.Send(GameEventBuilder.UseObject("operation", "release"));
            GuiPendulum.ShowFeedback(res.Feedback);

            mouseDown = false;
            joint.useLimits = false;

            //AssessmentManager.Instance.PrintSummary();
        }
         else if (Input.GetMouseButtonDown(0) || mouseDown)
        {
            if (!mouseDown)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 100))
                    if (hit.transform.name == PendulumWeight.name)
                    {
                        mouseDown = true;
                        mouseStart = Input.mousePosition;
                    }
            }

            if (mouseDown)
            {
                //relative mouse movement / (scaling factor for easy use) 
                var angle = ((mouseStart.x - Input.mousePosition.x) / (ropeLength * 10));
                //Everything above 140 degrees seems to freak out unity enormously, just don't allow it
                angle = Math.Min(Math.Max(-140f, angle), 140f);

                limitHinge(joint, angle);
            }
        } else if (Input.GetMouseButtonDown(1) )
        {
           if(slow)
                Time.timeScale = 1.0f;
            else
                Time.timeScale = 0.2f;

            Time.fixedDeltaTime = 0.02F * Time.timeScale;
            slow = !slow;
                       
        }

        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            setRopeLengthRelative(ropeLengthChangeStepSize);

        } else if(Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            setRopeLengthRelative(-ropeLengthChangeStepSize);
        }

        checkKeyboardInput();
        assertPosition();

        drawRope();
        adjustWeight();
        setText();

    }

    private void setText()
    {
        InfoTextPanel.GetComponent<TextMesh>().text = Math.Round(weight, 2).ToString() + " kg\n" + Math.Round(ropeLength * 100) + " cm";
    }

    /// <summary>
    /// Sometimes, the engine freaks out and sets insane values to the position of the weight. If that happens, this function should
    /// return the weight to its default location
    /// </summary>
    private void assertPosition()
    {
        if (!( between(-100, PendulumWeight.transform.position.x, 100))
            && between(-100, PendulumWeight.transform.position.y, 100)
            && between(-100, PendulumWeight.transform.position.z, 100)
            )
        {
            Debug.Log("Assertion Error: resetting position due to far off values");
            PendulumWeight.transform.position = defaultPosition;
        }
            
    }

    /// <summary>
    /// Returns true if lower <= value <= upper 
    /// </summary>
    /// <param name="lower"></param>
    /// <param name="value"></param>
    /// <param name="upper"></param>
    /// <returns></returns>
    private Boolean between(double lower, double value, double upper)
    {
        return (lower <= value) && (value <= upper);
     }

    private void checkKeyboardInput()
    {
        if (!Input.anyKeyDown)
            return;

        if (Input.GetKeyDown(KeyCode.W))
        {

        }
        if (Input.GetKeyDown(KeyCode.S))
        {

        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            weight -= weightChangeStepSize;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            weight += weightChangeStepSize;
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            StopWatch.SendMessage("SWStart");
        } 
        if (Input.GetKeyDown(KeyCode.E))
        {
            StopWatch.SendMessage("SWStop");

        }

        if (Input.GetKeyDown(KeyCode.Space))
            SceneManager.LoadScene("Laboratory");


    }

    private void CalculatorButtonPressed(Calculator.CalculatorButtonPressedEvent evt)
    {
        if (evt.Name != "enter")
            return;

        var ge = GameEventBuilder
            .EnvironmentVariable(name, "calculated_frequency", evt.CurrentNumber)
            .Add(GameEventBuilder.UseObject("operation", "submit-frequency"));

        var res = AssessmentManager.Instance.Send(ge);
        GuiPendulum.ShowFeedback(res.Feedback);

        if(res.Feedback.Length > 0)
            if(res.Feedback.Where(e => e.ColorCode == Evaluation.UnityInterface.EWS.ColorCode.Success).FirstOrDefault() != null)
            {
                GuiPendulum.Clear();
                var res2 = AssessmentManager.Instance.Send(GameEventBuilder.EnterSection("pendulum_maroon_2"));
                GuiPendulum.ShowFeedback(res2.Feedback);
            }

        
    }

    private void adjustWeight()
    {
        var obj = PendulumWeight.transform.Find("weight_obj/weight_gizmo");

        weight = Math.Min(Math.Max(weight, weightMin), weightMax);
        obj.transform.localScale = new Vector3(weight / 1000, weight / 1000, weight / 1000);

        //set the weight at the rigidbody (doesn't change anything physically, but.. you know...)
        PendulumWeight.GetComponent<Rigidbody>().mass = weight;

    }
    private void drawRope()
    {
        var startPos = PendulumWeight.transform.Find("weight_obj").transform.position;
        startPos.Set(startPos.x, startPos.y, startPos.z);
        DrawLine(startPos, StandRopeJoint.transform.position, new Color(0, 0, 0));
    }

    void setRopeLengthRelative(float value)
    {
        limitHinge(PendulumWeight.GetComponent<HingeJoint>(), 0);
        Vector3 currPos = PendulumWeight.transform.position;
        var obj = PendulumWeight.transform.Find("weight_obj");
        var pos = obj.transform.position;
        float newVal = Math.Max(-ropeMax, -ropeLength + value);
        newVal = Math.Min(newVal, -ropeMin);
        ropeLength = -newVal;

        pos.Set(transform.position.x, transform.position.y - ropeLength, transform.position.z);
        obj.transform.position = pos;

        var ec = GameEventBuilder.EnvironmentVariable(
            this.name,
            "theoretical_frequency",
            1 / (2 * Math.PI) * Math.Sqrt(Physics.gravity.magnitude / ropeLength)
        );

        AssessmentManager.Instance.Send(ec);
    }



    void DrawLine(Vector3 start, Vector3 end, Color color)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
        lr.startColor = lr.endColor = color;
        lr.startWidth = lr.endWidth = 0.001f;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        GameObject.Destroy(lastLine);
        lastLine = myLine;
    }

    void limitHinge(HingeJoint joint, float angle)
    {
        JointLimits jl = new JointLimits {
            min = angle,
            max = angle + 0.0001f // because it bugs out otherwise...
        };
        joint.useLimits = true;
        joint.limits = jl;
    }
    

}
