using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Calendar.NET
{
    #region Delegate Part
    /// <summary>
    /// A delegate for creating custom recurring frequencies
    /// </summary>
    /// <param name="evnt">The <see cref="IEvent"/> in question</param>
    /// <param name="day">The day in question</param>
    /// <returns>Should return a boolean value that indicates if the event should be rendered on the day passed in</returns>
    public delegate bool CustomRecurringFrequenciesHandler(IEvent evnt, DateTime day);
    public delegate void DateClicked(DateTime t);

    #endregion
    
    #region Enum 부분
    /// <summary>
    /// An enumeration describing various ways to view the calendar
    /// Enum값 Calendar를 표시하는 2가지 방법을 표시 CalendarView.
    /// </summary>
    public enum CalendarViews
    {
        /// <summary>
        /// Renders the Calendar in a month view
        /// </summary>
        Month = 1,
        /// <summary>
        /// Renders the Calendar in a day view
        /// </summary>
        Day = 2
    }

    /// <summary>
    /// An enumeration of built-in recurring event frequencies
    /// </summary>
    public enum RecurringFrequencies
    {
        /// <summary>
        /// Indicates that the event is non recurring will occur only one time
        /// </summary>
        None = 0,
        /// <summary>
        /// Indicates that the event will occur every day
        /// </summary>
        Daily = 1,
        /// <summary>
        /// Indicates that the event will occur every week day (Mon - Fri)
        /// </summary>
        EveryWeekday = 2,
        /// <summary>
        /// Indicates that the event will occur every Mon, Wed and Fri
        /// </summary>
        EveryMonWedFri = 3,
        /// <summary>
        /// Indicates that the event will occur every Tuesday and Thursday
        /// </summary>
        EveryTueThurs = 4,
        /// <summary>
        /// Indicates that the event will occur every week
        /// </summary>
        Weekly = 5,
        /// <summary>
        /// Indicates that the event will occur every month
        /// </summary>
        Monthly = 6,
        /// <summary>
        /// Indicates that the event will occur once a year, on the month and day specified
        /// </summary>
        Yearly = 7,
        /// <summary>
        /// Indicates that the event will occur every weekend on Saturday and Sunday
        /// </summary>
        EveryWeekend = 8,
        /// <summary>
        /// Indicates that the recuring schedule of this event is unique
        /// </summary>
        Custom = 99
    }


    #endregion
    
    #region Interface 부분

    /// <summary>
    /// An interface for creating event types
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// The Date that the event occurs
        /// </summary>
        DateTime Date { get; set; }
        /// <summary>
        /// True if the event is enabled, otherwise false
        /// </summary>
        bool Enabled { get; set; }
        /// <summary>
        /// A value indicating how often the event occurs
        /// </summary>
        RecurringFrequencies RecurringFrequency { get; set; }
        /// <summary>
        /// The name of the event
        /// </summary>
        string EventText { get; set; }

        string startTime { get; set; }

        string endTime { get; set; }

        string etc { get; set; }
        /// <summary>
        /// A value indicating the length of the event, in hours.
        /// </summary>
        float EventLengthInHours { get; set; }
        /// <summary>
        /// The color that the event show up in on the calendar
        /// </summary>
        Color EventColor { get; set; }
        /// <summary>
        /// The font describing the appearance of the event
        /// </summary>
        Font EventFont { get; set; }
        /// <summary>
        /// The text color of the event
        /// </summary>
        Color EventTextColor { get; set; }
        /// <summary>
        /// The ranking of the event that determines the order in which it is displayed on a particular day
        /// </summary>
        int Rank { get; set; }
        /// <summary>
        /// True if the time component of the date can be ignored
        /// </summary>
        bool IgnoreTimeComponent { get; set; }
        /// <summary>
        /// True if the event details cannot be modified
        /// </summary>
        bool ReadOnlyEvent { get; set; }
        /// <summary>
        /// True if a tooltip should be displayed when hovering over the event
        /// </summary>
        bool TooltipEnabled { get; set; }
        /// <summary>
        /// If this is a recurring event, set this to true to make the event show up only from the day specified forward
        /// </summary>
        bool ThisDayForwardOnly { get; set; }
        /// <summary>
        /// Set this to a custom function that will automatically determine if the event should be rendered on a given day.
        /// This is only executed if <see cref="RecurringFrequency"/> is set to custom.
        /// </summary>
        CustomRecurringFrequenciesHandler CustomRecurringFunction { get; set; }
        /// <summary>
        /// A function for cloning an event instance
        /// </summary>
        /// <returns>A cloned <see cref="IEvent"/></returns>
        IEvent Clone();
    }


    #endregion

    #region 캘린더 클래스
    /// <summary>1
    /// A Winforms Calendar Control
    /// </summary>
    public class Calendar : UserControl
    {
        #region 변수 선언부


        private DateTime _calendarDate;                 // 현재 날짜
        private Font _dayOfWeekFont;                    // 각종 폰트
        private Font _daysFont;
        private Font _todayFont;
        private Font _dateHeaderFont;
        private Font _dayViewTimeFont;
        private bool _showArrowControls;                // 달력이동을 할지 컨트롤 하는 부분                
        private bool _showTodayButton;                  // Today 버튼을 표시 여부
        private bool _showDateInHeader;                 // 위쪽 헤더 부분에 날짜 표시 여부
        private Button _todayBtn;                       // today 버튼
        private Button _leftBtn;                        // < 버튼
        private Button _rightBtn;                       // > 버튼
        private bool _showingToolTip;                   // 툴팁 표시 여부
        private bool _showEventTooltips;                // 이벤트의 툴팁 표시 여부
        private bool _loadPresetHolidays;               // Hollyday 표시 여부 
        private cEvent _cEvent;                         // 이벤트 
        private bool _showDisabledEvents;               // 이벤트 표시 여부
        private bool _showDashedBorderOnDisabledEvents;
        private bool _dimDisabledEvents;
        private bool _highlightCurrentDay;              // 오늘 날짜에 Highlight를 줄지 여부
        private CalendarViews _calendarView;            // 달력 표시 종류 일별로 보려면 Day 월별로 보려면 Month로, 우리는 Month만 봄
        private readonly ScrollPanel _scrollPanel;      // ScrollPanel

        private readonly List<IEvent> _events;                  // 이벤트를 담아두는 함수
        private readonly List<Rectangle> _rectangles;
        private readonly Dictionary<int, Point> _calendarDays;
        private readonly List<cEvent> _cEvents;
        private ContextMenuStrip _contextMenuStrip1;
        private System.ComponentModel.IContainer components;
        private ToolStripMenuItem _miProperties;

        private const int MarginSize = 20;

        private List<DateBox> dateBoxes;
        private ToolTip toolTip1;
        private int selectedDay = 0;


        public DateClicked dateClickEventHandler;
        #endregion

        #region 구조체 정의
        /// <summary>
        /// 이벤트에 관한 구조체 해당 이벤트가 차지하고 있는 Area와 Event, 
        /// </summary>
        struct cEvent
        {
            public Rectangle EventArea
            {
                get;
                set;
            }

            public IEvent Event
            {
                get;
                set;
            }

            public DateTime Date
            {
                get;
                set;
            }
        }

        private class DateBox
        {
            public int month;
            public int date;

            public bool isSelected;

            public Rectangle size;

            public CustomEvent WorkInfo;

            public DateBox()
            {
                month = 0;
                date = 0;

                isSelected = false;

                size = new Rectangle(0, 0, 0, 0);

                WorkInfo = new CustomEvent();
            }
        }

        #endregion

        #region 각종 설정 변수들

        /// <summary>
        /// Indicates the font for the times on the day view
        /// 
        /// </summary>
        public Font DayViewTimeFont
        {
            get { return _dayViewTimeFont; }
            set
            {
                _dayViewTimeFont = value;
                if (_calendarView == CalendarViews.Day)
                    _scrollPanel.Refresh();
                else Refresh();
            }
        }


        /// <summary>
        /// Indicates the type of calendar to render, Month or Day view
        /// </summary>
        public CalendarViews CalendarView
        {
            get { return _calendarView; }
            set
            {
                _calendarView = value;
                _scrollPanel.Visible = value == CalendarViews.Day;
                Refresh();
            }
        }

        /// <summary>
        /// Indicates whether today's date should be highlighted
        /// </summary>
        public bool HighlightCurrentDay
        {
            get { return _highlightCurrentDay; }
            set
            {
                _highlightCurrentDay = value;
                Refresh();
            }
        }

        /// <summary>
        /// Indicates whether events can be right-clicked and edited
        /// </summary>
        public bool AllowEditingEvents
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether disabled events will appear as "dimmed".
        /// This property is only used if <see cref="ShowDisabledEvents"/> is set to true.
        /// </summary>
        public bool DimDisabledEvents
        {
            get { return _dimDisabledEvents; }
            set
            {
                _dimDisabledEvents = value;
                Refresh();
            }
        }

        /// <summary>
        /// Indicates if a dashed border should show up around disabled events.
        /// This property is only used if <see cref="ShowDisabledEvents"/> is set to true.
        /// </summary>
        public bool ShowDashedBorderOnDisabledEvents
        {
            get { return _showDashedBorderOnDisabledEvents; }
            set
            {
                _showDashedBorderOnDisabledEvents = value;
                Refresh();
            }
        }

        /// <summary>
        /// Indicates whether disabled events should show up on the calendar control
        /// </summary>
        public bool ShowDisabledEvents
        {
            get { return _showDisabledEvents; }
            set
            {
                _showDisabledEvents = value;
                Refresh();
            }
        }

        /// <summary>
        /// Indicates whether Federal Holidays are automatically preloaded onto the calendar
        /// </summary>
        public bool LoadPresetHolidays
        {
            get { return _loadPresetHolidays; }
            set
            {
                _loadPresetHolidays = value;
                if (_loadPresetHolidays)
                {
                    _events.Clear();
                    //PresetHolidays();
                    Refresh();
                }
                else
                {
                    _events.Clear();
                    Refresh();
                }
            }
        }

        /// <summary>
        /// Indicates whether hovering over an event will display a tooltip of the event
        /// </summary>
        public bool ShowEventTooltips
        {
            get { return _showEventTooltips; }
            set { _showEventTooltips = value; }//_eventTip.Visible = false; }
        }

        /// <summary>
        /// Get or Set this value to the Font you wish to use to render the date in the upper right corner
        /// </summary>
        public Font DateHeaderFont
        {
            get { return _dateHeaderFont; }
            set
            {
                _dateHeaderFont = value;
                Refresh();
            }
        }

        /// <summary>
        /// Indicates whether the date should be displayed in the upper right hand corner of the calendar control
        /// </summary>
        public bool ShowDateInHeader
        {
            get { return _showDateInHeader; }
            set
            {
                _showDateInHeader = value;
                if (_calendarView == CalendarViews.Day)
                    ResizeScrollPanel();

                Refresh();
            }
        }

        /// <summary>
        /// Indicates whether the calendar control should render the previous/next month buttons
        /// </summary>
        public bool ShowArrowControls
        {
            get { return _showArrowControls; }
            set
            {
                _showArrowControls = value;
                _leftBtn.Visible = value;
                _rightBtn.Visible = value;
                if (_calendarView == CalendarViews.Day)
                    ResizeScrollPanel();
                Refresh();
            }
        }

        /// <summary>
        /// Indicates whether the calendar control should render the Today button
        /// </summary>
        public bool ShowTodayButton
        {
            get { return _showTodayButton; }
            set
            {
                _showTodayButton = value;
                _todayBtn.Visible = value;
                if (_calendarView == CalendarViews.Day)
                    ResizeScrollPanel();
                Refresh();
            }
        }

        /// <summary>
        /// The font used to render the Today button
        /// </summary>
        public Font TodayFont
        {
            get { return _todayFont; }
            set
            {
                _todayFont = value;
                Refresh();
            }
        }

        /// <summary>
        /// The font used to render the number days on the calendar
        /// </summary>
        public Font DaysFont
        {
            get { return _daysFont; }
            set
            {
                _daysFont = value;
                Refresh();
            }
        }

        /// <summary>
        /// The font used to render the days of the week text
        /// </summary>
        public Font DayOfWeekFont
        {
            get { return _dayOfWeekFont; }
            set
            {
                _dayOfWeekFont = value;
                Refresh();
            }
        }

        /// <summary>
        /// The Date that the calendar is currently showing
        /// </summary>
        public DateTime CalendarDate
        {
            get { return _calendarDate; }
            set
            {
                _calendarDate = value;
                Refresh();
            }
        }

        #endregion

        /// <summary>
        /// 생성자
        /// </summary>
        public Calendar()
        {
            InitializeComponent();
            _calendarDate = DateTime.Now;
            _dayOfWeekFont = new Font("Arial", 10, FontStyle.Regular);
            _daysFont = new Font("Arial", 10, FontStyle.Regular);
            _todayFont = new Font("Arial", 10, FontStyle.Bold);
            _dateHeaderFont = new Font("Arial", 12, FontStyle.Bold);
            _dayViewTimeFont = new Font("Arial", 10, FontStyle.Bold);
            _showArrowControls = true;
            _showDateInHeader = true;
            _showTodayButton = true;
            _showingToolTip = false;
            _cEvent = new cEvent();
            _showDisabledEvents = false;
            _showDashedBorderOnDisabledEvents = true;
            _dimDisabledEvents = true;
            AllowEditingEvents = true;
            _highlightCurrentDay = true;
            _calendarView = CalendarViews.Month;
            _scrollPanel = new ScrollPanel();

            _events = new List<IEvent>();
            _rectangles = new List<Rectangle>();
            _calendarDays = new Dictionary<int, Point>();
            _cEvents = new List<cEvent>();
            _showEventTooltips = true;
            //_eventTip = new EventToolTip { Visible = false };

            dateBoxes = new List<DateBox>();

            //Controls.Add(_eventTip);

            LoadPresetHolidays = true;

            _scrollPanel.Visible = false;
            Controls.Add(_scrollPanel);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this._todayBtn = new System.Windows.Forms.Button();
            this._leftBtn = new System.Windows.Forms.Button();
            this._rightBtn = new System.Windows.Forms.Button();
            this._contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._miProperties = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this._contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // _todayBtn
            // 
            this._todayBtn.Location = new System.Drawing.Point(20, 20);
            this._todayBtn.Name = "_todayBtn";
            this._todayBtn.Size = new System.Drawing.Size(72, 29);
            this._todayBtn.TabIndex = 0;
            this._todayBtn.Text = "Today";
            this._todayBtn.Click += _todayBtn_Click;
            // 
            // _leftBtn
            // 
            this._leftBtn.Location = new System.Drawing.Point(98, 20);
            this._leftBtn.Name = "_leftBtn";
            this._leftBtn.Size = new System.Drawing.Size(42, 29);
            this._leftBtn.TabIndex = 1;
            this._leftBtn.Text = "<";
            this._leftBtn.Click += _leftBtn_Click;
            // 
            // _rightBtn
            // 
            this._rightBtn.Location = new System.Drawing.Point(138, 20);
            this._rightBtn.Name = "_rightBtn";
            this._rightBtn.Size = new System.Drawing.Size(42, 29);
            this._rightBtn.TabIndex = 1;
            this._rightBtn.Text = ">";
            this._rightBtn.Click += _rightBtn_Click;
            // 
            // _contextMenuStrip1
            // 
            this._contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._miProperties});
            this._contextMenuStrip1.Name = "_contextMenuStrip1";
            this._contextMenuStrip1.Size = new System.Drawing.Size(137, 26);
            // 
            // _miProperties
            // 
            this._miProperties.Name = "_miProperties";
            this._miProperties.Size = new System.Drawing.Size(136, 22);
            this._miProperties.Text = "Properties...";
            // 
            // Calendar
            // 
            this.Controls.Add(this._leftBtn);
            this.Controls.Add(this._rightBtn);
            this.Controls.Add(this._todayBtn);
            this.DoubleBuffered = true;
            this.Name = "Calendar";
            this.Size = new System.Drawing.Size(512, 440);
            this.Load += new System.EventHandler(this.CalendarLoad);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.CalendarPaint);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.CalendarMouseClick);
            this.MouseDoubleClick += Calendar_MouseDoubleClick;
            //this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.CalendarMouseMove);
            this.Resize += new System.EventHandler(this.CalendarResize);
            this._contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

            this.toolTip1.InitialDelay = 1000;
            this.toolTip1.AutomaticDelay = 1000;
            //this.toolTip1.IsBalloon = true;
        }

        #region 이벤트 처리 함수들

        bool show = false;


        // Calender 위에서 마우스가 움직일때 발생하는 이벤트
        private void CalendarMouseMove(object sender, MouseEventArgs e)
        {
            if (!_showEventTooltips)
                return;

            // 모든 데이타박스 검색
            for (int i = 0; i < dateBoxes.Count; i++)
            {
                if (dateBoxes[i].size.Contains(new Point(e.X, e.Y)))
                {
                    for (int j = 0; j < _events.Count; j++)
                    {
                        if (dateBoxes[i].date == _events[j].Date.Day)
                        {
                            this.toolTip1.Show(_events[j].etc, this);
                        }
                        else
                        {

                        }
                    }
                }
            }
        }

        // 마우스 클릭에 대한 이벤트
        private void CalendarMouseClick(object sender, MouseEventArgs e)
        {

            // 마우스 왼쪽 버튼
            if (e.Button == MouseButtons.Left && AllowEditingEvents)
            {
                // 한번 클릭하면 색이 변함 두번 클릭하면 특정 이벤트 실행
                for (int i = 0; i < dateBoxes.Count; i++)
                {
                    if (dateBoxes[i].size.Contains(new Point(e.X, e.Y)))
                    {

                        if (selectedDay == dateBoxes[i].date)
                        {
                            // 새 폼을 생성하는 부분

                            dateClickEventHandler(new DateTime(CalendarDate.Year, CalendarDate.Month, dateBoxes[i].date));
                        }

                        selectedDay = dateBoxes[i].date;

                        Refresh();

                    }
                }
            }

            // 마우스 오른쪽 버튼 이벤트 -> 나중에 추가
            if (e.Button == MouseButtons.Right && AllowEditingEvents)
            {
                // 마우스 오른쪽 클릭에 대한 로직 
            }
        }

        // 더블클릭
        private void Calendar_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < dateBoxes.Count; i++)
            {
                if (dateBoxes[i].size.Contains(new Point(e.X, e.Y)))
                {
                    if (selectedDay == dateBoxes[i].date)
                    {
                        // 새 폼을 생성하는 부분
                        dateClickEventHandler(new DateTime(CalendarDate.Year, CalendarDate.Month, dateBoxes[i].date));
                    }

                    selectedDay = dateBoxes[i].date;

                    Refresh();
                }
            }
        }

        // > 모양의 버튼을 눌렀을때의 클릭 이벤트
        private void _rightBtn_Click(object sender, EventArgs e)
        {
            selectedDay = 0;
            if (_calendarView == CalendarViews.Day)
                _calendarDate = _calendarDate.AddDays(1);
            else if (_calendarView == CalendarViews.Month)
                _calendarDate = _calendarDate.AddMonths(1);
            Refresh();
        }


        // < 모양의 버튼을 눌렀을때의 이벤트 처리
        private void _leftBtn_Click(object sender, EventArgs e)
        {
            selectedDay = 0;
            if (_calendarView == CalendarViews.Month)
                _calendarDate = _calendarDate.AddMonths(-1);
            else if (_calendarView == CalendarViews.Day)
                _calendarDate = _calendarDate.AddDays(-1);
            Refresh();
        }

        // today 버튼을 눌렀을때의 이벤트 처리
        private void _todayBtn_Click(object sender, EventArgs e)
        {
            _calendarDate = DateTime.Now;
            Refresh();
        }


        #endregion

        #region public 함수들
        /// <summary>
        /// Adds an event to the calendar
        /// </summary>
        /// <param name="calendarEvent">The <see cref="IEvent"/> to add to the calendar</param>
        public void AddEvent(IEvent calendarEvent)
        {
            _events.Add(calendarEvent);
            Refresh();
        }

        /// <summary>
        /// 근무관련된 정보를 입력합니다.
        /// </summary>
        /// <param name="Text"> 근무 내용</param>
        /// <param name="Date"> 근무 시간</param>
        public void AddWork(String startTime, String endTime, String etc, DateTime Date)
        {
            bool existEvent = false;            // 하루에 근무시간은 한번 있어야함. 이를 판단하기 위해 존재하는 변수

            var ce = new CustomEvent();
            ce.IgnoreTimeComponent = false;
            ce.startTime = startTime;
            ce.endTime = endTime;
            ce.etc = etc;
            ce.Date = Date;
            ce.EventLengthInHours = 2f;
            ce.RecurringFrequency = RecurringFrequencies.None;
            ce.EventFont = new Font("Verdana", 8, FontStyle.Regular);
            ce.Enabled = true;
            ce.TooltipEnabled = false;
            ce.EventColor = Color.FromArgb(255, 255, 255, 255);
            ce.EventTextColor = Color.Black;

            for(int i = 0;i < _events.Count; i++)
            {
                if (_events[i].Date.Year == _calendarDate.Year && _events[i].Date.Month == _calendarDate.Month
                    && _events[i].Date.Day == Date.Day)
                {
                    _events[i] = ce;
                    existEvent = true;
                }
                
            }
            
            if (!existEvent)            // 해당 날짜에 이벤트가 존재하면 추가하지 않는다.
                _events.Add(ce);

            
        }

        /// <summary>
        /// 하루 일 시작 시간을 입력합니다.
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="Date"></param>
        public void AddWorkStartTime(String startTime, DateTime Date)
        {
            for(int i = 0;i < _events.Count; i++)
            {
                if(_events[i].Date.Year == _calendarDate.Year && _events[i].Date.Month == _calendarDate.Month
                    && _events[i].Date.Day == Date.Day)
                {
                    _events[i].startTime = startTime;
                    Refresh();
                    return;
                }
            }
            AddWork(startTime, "", "", Date);
            Refresh();
            
        }

       /// <summary>
       /// 근무 마치는 시간을 입력합니다.
       /// </summary>
       /// <param name="endTime"></param>
       /// <param name="Date"></param>
        public void AddWorkEndTime(String endTime, DateTime Date)
        {
            bool existEvent = false;

            for (int i = 0; i < _events.Count; i++)
            {
                if (_events[i].Date.Year == _calendarDate.Year && _events[i].Date.Month == _calendarDate.Month
                    && _events[i].Date.Day == Date.Day)
                {
                    existEvent = true;
                    if (_events[i].startTime == "")         // 시작시간이 입력되지 않을 경우 그냥 종료
                        return;

                    _events[i].endTime = endTime;
                    Refresh();

                    return;
                }
            }
            if(existEvent)
                AddWork("", endTime, "", Date);
        }

        /// <summary>
        /// 비고란을 입력합니다.
        /// </summary>
        /// <param name="etc"></param>
        /// <param name="Date"></param>
        public void AddEtc(String etc, DateTime Date)
        {
            for (int i = 0; i < _events.Count; i++)
            {
                if (_events[i].Date.Year == _calendarDate.Year && _events[i].Date.Month == _calendarDate.Month
                    && _events[i].Date.Day == Date.Day)
                {
                    
                    _events[i].etc = etc;
                    Refresh();

                    return;
                }
            }
        }

        /// <summary>
        /// Removes an event from the calendar
        /// </summary>
        /// <param name="calendarEvent">The <see cref="IEvent"/> to remove to the calendar</param>
        public void RemoveEvent(IEvent calendarEvent)
        {
            _events.Remove(calendarEvent);
            Refresh();
        }

        #endregion

        #region 코어함수

        private void CalendarLoad(object sender, EventArgs e)
        {
            if (Parent != null)
                Parent.Resize += ParentResize;
            ResizeScrollPanel();
        }

        private void CalendarPaint(object sender, PaintEventArgs e)
        {
            if (_showingToolTip)
                return;

            if (_calendarView == CalendarViews.Month)
                RenderMonthCalendar(e);
            if (_calendarView == CalendarViews.Day)
                RenderDayCalendar(e);
        }


        private void ParentResize(object sender, EventArgs e)
        {
            ResizeScrollPanel();
            Refresh();
        }
        
        private DateTime LastDayOfWeekInMonth(DateTime day, DayOfWeek dow)
        {
            DateTime lastDay = new DateTime(day.Year, day.Month, 1).AddMonths(1).AddDays(-1);
            DayOfWeek lastDow = lastDay.DayOfWeek;

            int diff = dow - lastDow;

            if (diff > 0) diff -= 7;

            System.Diagnostics.Debug.Assert(diff <= 0);

            return lastDay.AddDays(diff);
        }

        private int Max(params float[] value)
        {
            return (int)value.Max(i => Math.Ceiling(i));
        }

        private bool DayForward(IEvent evnt, DateTime day)
        {
            if (evnt.ThisDayForwardOnly)
            {
                int c = DateTime.Compare(day, evnt.Date);

                if (c >= 0)
                    return true;

                return false;
            }

            return true;
        }

        // 이미지 요청
        internal Bitmap RequestImage()
        {
            const int cellHourWidth = 60;
            const int cellHourHeight = 30;
            var bmp = new Bitmap(ClientSize.Width, cellHourWidth * 24);
            Graphics g = Graphics.FromImage(bmp);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            var dt = new DateTime(_calendarDate.Year, _calendarDate.Month, _calendarDate.Day, 0, 0, 0);
            int xStart = 0;
            int yStart = 0;

            g.DrawRectangle(Pens.Black, 0, 0, ClientSize.Width - MarginSize * 2 - 2, cellHourHeight * 24);
            for (int i = 0; i < 24; i++)
            {
                var textWidth = (int)g.MeasureString(dt.ToString("htt").ToLower(), _dayViewTimeFont).Width;
                g.DrawRectangle(Pens.Black, xStart, yStart, cellHourWidth, cellHourHeight);
                g.DrawLine(Pens.Black, xStart + cellHourWidth, yStart + cellHourHeight,
                           ClientSize.Width - MarginSize * 2 - 3, yStart + cellHourHeight);
                g.DrawLine(Pens.DarkGray, xStart + cellHourWidth, yStart + cellHourHeight / 2,
                           ClientSize.Width - MarginSize * 2 - 3, yStart + cellHourHeight / 2);

                g.DrawString(dt.ToString("htt").ToLower(), _dayViewTimeFont, Brushes.Black, xStart + cellHourWidth - textWidth, yStart);
                yStart += cellHourHeight;
                dt = dt.AddHours(1);
            }

            dt = new DateTime(_calendarDate.Year, _calendarDate.Month, _calendarDate.Day, 23, 59, 0);

            List<IEvent> evnts = _events.Where(evnt => NeedsRendering(evnt, dt)).ToList().OrderBy(d => d.Date).ToList();

            xStart = cellHourWidth + 1;
            yStart = 0;

            g.Clip = new Region(new Rectangle(0, 0, ClientSize.Width - MarginSize * 2 - 2, cellHourHeight * 24));
            _cEvents.Clear();
            for (int i = 0; i < 24; i++)
            {
                dt = new DateTime(_calendarDate.Year, _calendarDate.Month, _calendarDate.Day, 0, 0, 0);
                dt = dt.AddHours(i);
                foreach (var evnt in evnts)
                {
                    TimeSpan ts = TimeSpan.FromHours(evnt.EventLengthInHours);

                    if (evnt.Date.Ticks >= dt.Ticks && evnt.Date.Ticks < dt.Add(ts).Ticks && evnt.EventLengthInHours > 0 && i >= evnt.Date.Hour)
                    {
                        int divisor = evnt.Date.Minute == 0 ? 1 : 60 / evnt.Date.Minute;
                        Color clr = Color.FromArgb(175, evnt.EventColor.R, evnt.EventColor.G, evnt.EventColor.B);
                        g.FillRectangle(new SolidBrush(GetFinalBackColor()), xStart, yStart + cellHourHeight / divisor + 1, ClientSize.Width - MarginSize * 2 - cellHourWidth - 3, cellHourHeight * ts.Hours - 1);
                        g.FillRectangle(new SolidBrush(clr), xStart, yStart + cellHourHeight / divisor + 1, ClientSize.Width - MarginSize * 2 - cellHourWidth - 3, cellHourHeight * ts.Hours - 1);
                        g.DrawString(evnt.startTime, evnt.EventFont, new SolidBrush(evnt.EventTextColor), xStart, yStart + cellHourHeight / divisor);
                        g.DrawString(evnt.endTime, evnt.EventFont, new SolidBrush(evnt.EventTextColor), xStart, (yStart * 2) + cellHourHeight / divisor);
                        g.DrawString(evnt.etc, evnt.EventFont, new SolidBrush(evnt.EventTextColor), xStart, (yStart * 3) + cellHourHeight / divisor);
                                                
                        var ces = new cEvent
                        {
                            Event = evnt,
                            Date = dt,
                            EventArea = new Rectangle(xStart, yStart + cellHourHeight / divisor + 1,
                                                                   ClientSize.Width - MarginSize * 2 - cellHourWidth - 3,
                                                                   cellHourHeight * ts.Hours)
                        };
                        _cEvents.Add(ces);
                    }
                }
                yStart += cellHourHeight;
            }

            g.Dispose();
            return bmp;
        }

        private Color GetFinalBackColor()
        {
            Control c = this;

            while (c != null)
            {
                if (c.BackColor != Color.Transparent)
                    return c.BackColor;
                c = c.Parent;
            }

            return Color.Transparent;
        }

        private void ResizeScrollPanel()
        {
            int controlsSpacing = ((!_showTodayButton) && (!_showDateInHeader) && (!_showArrowControls)) ? 0 : 30;

            _scrollPanel.Location = new Point(MarginSize, MarginSize + controlsSpacing);
            _scrollPanel.Size = new Size(ClientSize.Width - MarginSize * 2 - 1, ClientSize.Height - MarginSize - 1 - controlsSpacing);
        }

        private void RenderDayCalendar(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (_showDateInHeader)
            {
                SizeF dateHeaderSize = g.MeasureString(
                    _calendarDate.ToString("MMMM") + " " + _calendarDate.Day.ToString(CultureInfo.InvariantCulture) +
                    ", " + _calendarDate.Year.ToString(CultureInfo.InvariantCulture), DateHeaderFont);

                g.DrawString(
                    _calendarDate.ToString("MMMM") + " " + _calendarDate.Day.ToString(CultureInfo.InvariantCulture) +
                    ", " + _calendarDate.Year.ToString(CultureInfo.InvariantCulture),
                    _dateHeaderFont, Brushes.Black, ClientSize.Width - MarginSize - dateHeaderSize.Width,
                    MarginSize);
            }
        }


        // 해당 달의 캘린더를 그림
        private void RenderMonthCalendar(PaintEventArgs e)
        {
            _calendarDays.Clear();
            _cEvents.Clear();
            dateBoxes.Clear();

            var bmp = new Bitmap(ClientSize.Width, ClientSize.Height);
            Graphics g = Graphics.FromImage(bmp);
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            SizeF sunSize = g.MeasureString("Sun", _dayOfWeekFont);
            SizeF monSize = g.MeasureString("Mon", _dayOfWeekFont);
            SizeF tueSize = g.MeasureString("Tue", _dayOfWeekFont);
            SizeF wedSize = g.MeasureString("Wed", _dayOfWeekFont);
            SizeF thuSize = g.MeasureString("Thu", _dayOfWeekFont);
            SizeF friSize = g.MeasureString("Fri", _dayOfWeekFont);
            SizeF satSize = g.MeasureString("Sat", _dayOfWeekFont);
            SizeF dateHeaderSize = g.MeasureString(
                _calendarDate.ToString("MMMM") + " " + _calendarDate.Year.ToString(CultureInfo.InvariantCulture), _dateHeaderFont);
            int headerSpacing = Max(sunSize.Height, monSize.Height, tueSize.Height, wedSize.Height, thuSize.Height, friSize.Height,
                          satSize.Height) + 5;
            int controlsSpacing = ((!_showTodayButton) && (!_showDateInHeader) && (!_showArrowControls)) ? 0 : 30;
            int cellWidth = (ClientSize.Width - MarginSize * 2) / 7;
            int numWeeks = NumberOfWeeks(_calendarDate.Year, _calendarDate.Month);
            int cellHeight = (ClientSize.Height - MarginSize * 2 - headerSpacing - controlsSpacing) / numWeeks;
            int xStart = MarginSize;
            int yStart = MarginSize;
            DayOfWeek startWeekEnum = new DateTime(_calendarDate.Year, _calendarDate.Month, 1).DayOfWeek;
            int startWeek = ((int)startWeekEnum) + 1;
            int rogueDays = startWeek - 1;

            yStart += headerSpacing + controlsSpacing;

            int counter = 1;
            int counter2 = 1;

            bool first = false;
            bool first2 = false;

            
            _todayBtn.Location = new Point(MarginSize, MarginSize);

            // 달력 그리는 부분
            for (int y = 0; y < numWeeks; y++)
            {
                for (int x = 0; x < 7; x++)
                {
                    if (rogueDays == 0 && counter <= DateTime.DaysInMonth(_calendarDate.Year, _calendarDate.Month))
                    {
                        if (!_calendarDays.ContainsKey(counter))
                            _calendarDays.Add(counter, new Point(xStart, (int)(yStart + 2f + g.MeasureString(counter.ToString(CultureInfo.InvariantCulture), _daysFont).Height)));

                        // Today
                        if (_calendarDate.Year == DateTime.Now.Year && _calendarDate.Month == DateTime.Now.Month
                         && counter == DateTime.Now.Day && _highlightCurrentDay)
                        {
                            g.FillRectangle(new SolidBrush(Color.FromArgb(234, 234, 234)), xStart, yStart, cellWidth, cellHeight);
                        }

                        if(counter == selectedDay)
                        {
                            g.FillRectangle(new SolidBrush(Color.AliceBlue), xStart, yStart, cellWidth, cellHeight);
                        }

                        // Date 이벤트 부여를 위한 날짜 + Rectangle 구조 저장
                        DateBox dateInfo = new DateBox();
                        dateInfo.month = _calendarDate.Month;
                        dateInfo.date = counter;
                        dateInfo.size = new Rectangle(xStart, yStart, cellWidth, cellHeight);
                        dateBoxes.Add(dateInfo);

                        // 날짜 그리기
                        if (first == false)
                        {
                            first = true;
                            if (_calendarDate.Year == DateTime.Now.Year && _calendarDate.Month == DateTime.Now.Month
                         && counter == DateTime.Now.Day)
                            {
                                g.DrawString(counter.ToString(CultureInfo.InvariantCulture), _todayFont, Brushes.Black, xStart + 5, yStart + 2);
                            }
                            else
                            {
                                g.DrawString(counter.ToString(CultureInfo.InvariantCulture), _daysFont, Brushes.Black, xStart + 5, yStart + 2);
                            }
                        }
                        else
                        {
                            if (_calendarDate.Year == DateTime.Now.Year && _calendarDate.Month == DateTime.Now.Month
                         && counter == DateTime.Now.Day)
                            {
                                g.DrawString(counter.ToString(CultureInfo.InvariantCulture), _todayFont, Brushes.Black, xStart + 5, yStart + 2);
                            }
                            else
                            {
                                g.DrawString(counter.ToString(CultureInfo.InvariantCulture), _daysFont, Brushes.Black, xStart + 5, yStart + 2);
                            }
                        }
                        counter++;
                    }
                    else if (rogueDays > 0)
                    {
                        int dm =
                            DateTime.DaysInMonth(_calendarDate.AddMonths(-1).Year, _calendarDate.AddMonths(-1).Month) -
                            rogueDays + 1;
                        g.DrawString(dm.ToString(CultureInfo.InvariantCulture), _daysFont, new SolidBrush(Color.FromArgb(170, 170, 170)), xStart + 5, yStart + 2);
                        rogueDays--;
                    }

                    // 달력 선 그리기
                    
                    g.DrawRectangle(Pens.DarkGray, xStart, yStart, cellWidth, cellHeight);
                    

                    if (rogueDays == 0 && counter > DateTime.DaysInMonth(_calendarDate.Year, _calendarDate.Month))
                    {
                        if (first2 == false)
                            first2 = true;
                        else
                        {
                            if (counter2 == 1)
                            {
                                g.DrawString(_calendarDate.AddMonths(1).ToString("MMM") + " " + counter2.ToString(CultureInfo.InvariantCulture), _daysFont,
                                            new SolidBrush(Color.FromArgb(170, 170, 170)), xStart + 5, yStart + 2);
                            }
                            else
                            {
                                g.DrawString(counter2.ToString(CultureInfo.InvariantCulture), _daysFont,
                                             new SolidBrush(Color.FromArgb(170, 170, 170)), xStart + 5, yStart + 2);
                            }
                            counter2++;
                            counter++;
                        }
                    }
                    xStart += cellWidth;
                }
                xStart = MarginSize;
                yStart += cellHeight;
            }
            xStart = MarginSize + ((cellWidth - (int)sunSize.Width) / 2);
            yStart = MarginSize + controlsSpacing;

            g.DrawString("Sun", _dayOfWeekFont, Brushes.Red, xStart, yStart);
            xStart = MarginSize + ((cellWidth - (int)monSize.Width) / 2) + cellWidth;
            g.DrawString("Mon", _dayOfWeekFont, Brushes.Black, xStart, yStart);

            xStart = MarginSize + ((cellWidth - (int)tueSize.Width) / 2) + cellWidth * 2;
            g.DrawString("Tue", _dayOfWeekFont, Brushes.Black, xStart, yStart);

            xStart = MarginSize + ((cellWidth - (int)wedSize.Width) / 2) + cellWidth * 3;
            g.DrawString("Wed", _dayOfWeekFont, Brushes.Black, xStart, yStart);

            xStart = MarginSize + ((cellWidth - (int)thuSize.Width) / 2) + cellWidth * 4;
            g.DrawString("Thu", _dayOfWeekFont, Brushes.Black, xStart, yStart);

            xStart = MarginSize + ((cellWidth - (int)friSize.Width) / 2) + cellWidth * 5;
            g.DrawString("Fri", _dayOfWeekFont, Brushes.Black, xStart, yStart);

            xStart = MarginSize + ((cellWidth - (int)satSize.Width) / 2) + cellWidth * 6;
            g.DrawString("Sat", _dayOfWeekFont, Brushes.Red, xStart, yStart);

            // 년도 쓰는 부분
            if (_showDateInHeader)
            {
                g.DrawString(
                    _calendarDate.Year.ToString(CultureInfo.InvariantCulture) + "년 " + _calendarDate.ToString("MMMM"),
                    _dateHeaderFont, Brushes.Black, ClientSize.Width / 2,
                    MarginSize);

                // 범례 부분
                Font ImageInfoFont = new Font("Arial", 8, FontStyle.Regular);

                g.DrawRectangle(Pens.Black, new Rectangle(ClientSize.Width - 105, MarginSize - 16, 80, 46));

                
                try {
                    Bitmap start = new Bitmap(Application.StartupPath + @"\image\start.png");
                    Rectangle startSize = new Rectangle(ClientSize.Width - 100, MarginSize - 12, 10, 12);
                    g.DrawImage(start, startSize);
                }
                catch (Exception ex)
                {

                }
                
                g.DrawString("근무 시작", ImageInfoFont, Brushes.Black, ClientSize.Width - 100 + 12, MarginSize - 12);

                
                try { 
                    Bitmap end = new Bitmap(Application.StartupPath + @"\image\end.png");
                    Rectangle endSize = new Rectangle(ClientSize.Width - 100, MarginSize + 2, 10, 12);
                    g.DrawImage(end, endSize);
                }
                catch (Exception ex)
                {

                }
                g.DrawString("근무 종료", ImageInfoFont, Brushes.Black, ClientSize.Width - 100 + 12, MarginSize + 2);
                
                try {
                    Bitmap etc = new Bitmap(Application.StartupPath + @"\image\etc.png");
                    Rectangle etcSize = new Rectangle(ClientSize.Width - 100, MarginSize + 16, 10, 12);
                    g.DrawImage(etc, etcSize);
                }
                catch(Exception ex)
                {

                }

                g.DrawString("비고", ImageInfoFont, Brushes.Black, ClientSize.Width - 100 + 12, MarginSize + 16);
            
            }

            // 해당 년과 달의 이벤트를 찾아서 그리는 부분
            for (int i = 1; i <= DateTime.DaysInMonth(_calendarDate.Year, _calendarDate.Month); i++)
            {
                int renderOffsetY = 0;
                
                foreach (IEvent v in _events)
                {
                    var dt = new DateTime(_calendarDate.Year, _calendarDate.Month, i, 23, 59, _calendarDate.Second);
                    // 해당 Date에 이벤트가 존재하면
                    if (NeedsRendering(v, dt))
                    {
                        int alpha = 255;
                        if (!v.Enabled && _dimDisabledEvents)
                            alpha = 64;
                        Color alphaColor = Color.FromArgb(alpha, v.EventColor.R, v.EventColor.G, v.EventColor.B);

                        int offsetY = renderOffsetY;
                        Region r = g.Clip;
                        Point point = _calendarDays[i];
                        SizeF sz = g.MeasureString(v.startTime, v.EventFont);
                        int yy = point.Y - 1;
                        int xx = ((cellWidth - (int)sz.Width) / 2) + point.X;

                        if (sz.Width > cellWidth)
                            xx = point.X;
                        if (renderOffsetY + sz.Height > cellHeight - 10)
                            continue;
                        g.Clip = new Region(new Rectangle(point.X + 1, point.Y + offsetY, cellWidth - 1, (int)(sz.Height * 4)));
                        //g.FillRectangle(new SolidBrush(alphaColor), point.X + 1, point.Y + offsetY, cellWidth - 1, sz.Height);
                        if (!v.Enabled && _showDashedBorderOnDisabledEvents)
                        {
                            var p = new Pen(new SolidBrush(Color.FromArgb(255, 0, 0, 0))) { DashStyle = DashStyle.Dash };
                            g.DrawRectangle(p, point.X + 1, point.Y + offsetY, cellWidth - 2, sz.Height - 1);
                        }
                        // 근무 시작 이미지와 글씨 입력
                        Bitmap start = new Bitmap(Application.StartupPath + @"\image\start.png");
                        Rectangle startSize = new Rectangle(xx, yy + offsetY, 10, (int)(sz.Height + 1));
                        g.DrawImage(start, startSize);
                        g.DrawString(v.startTime, v.EventFont, new SolidBrush(v.EventTextColor), xx + 10, yy + offsetY);

                        // 근무 마침 이미지와 글씨 입력
                        Bitmap end = new Bitmap(Application.StartupPath + @"\image\end.png");
                        Rectangle endSize = new Rectangle(xx, yy + (int)(sz.Height + 1), 10, (int)(sz.Height + 1));
                        g.DrawImage(end, endSize);
                        g.DrawString(v.endTime, v.EventFont, new SolidBrush(v.EventTextColor), xx + 10, yy + (int)(sz.Height + 1));

                        // 비고란 이미지와 글씨 입력
                        Bitmap etc = new Bitmap(Application.StartupPath + @"\image\etc.png");
                        Rectangle etcSize = new Rectangle(xx, yy + (int)((sz.Height + 1) * 2), 10, (int)(sz.Height + 1));
                        g.DrawImage(etc, etcSize);
                        g.DrawString(v.etc, v.EventFont, new SolidBrush(v.EventTextColor), xx + 10, yy + (int)((sz.Height + 1) * 2));
                        
                        g.Clip = r;

                        var evs = new cEvent
                        {
                            EventArea = new Rectangle(point.X + 1, point.Y + offsetY, cellWidth - 1, (int)sz.Height),
                            Event = v,
                            Date = dt
                        };

                        _cEvents.Add(evs);
                        renderOffsetY += (int)sz.Height + 1;
                    }
                }
            }
            _rectangles.Clear();

            g.Dispose();
            e.Graphics.DrawImage(bmp, 0, 0, ClientSize.Width, ClientSize.Height);
            bmp.Dispose();
        }

        // 이벤트를 그릴필요가 있는지 검사하는 함수
        private bool NeedsRendering(IEvent evnt, DateTime day)
        {
            if (!evnt.Enabled && !_showDisabledEvents)
                return false;

            DayOfWeek dw = evnt.Date.DayOfWeek;

            if (evnt.RecurringFrequency == RecurringFrequencies.Daily)
            {
                return DayForward(evnt, day);
            }
            if (evnt.RecurringFrequency == RecurringFrequencies.Weekly && day.DayOfWeek == dw)
            {
                return DayForward(evnt, day);
            }
            if (evnt.RecurringFrequency == RecurringFrequencies.EveryWeekend && (day.DayOfWeek == DayOfWeek.Saturday ||
                day.DayOfWeek == DayOfWeek.Sunday))
                return DayForward(evnt, day);
            if (evnt.RecurringFrequency == RecurringFrequencies.EveryMonWedFri && (day.DayOfWeek == DayOfWeek.Monday ||
                day.DayOfWeek == DayOfWeek.Wednesday || day.DayOfWeek == DayOfWeek.Friday))
            {
                return DayForward(evnt, day);
            }
            if (evnt.RecurringFrequency == RecurringFrequencies.EveryTueThurs && (day.DayOfWeek == DayOfWeek.Thursday ||
                day.DayOfWeek == DayOfWeek.Tuesday))
                return DayForward(evnt, day);
            if (evnt.RecurringFrequency == RecurringFrequencies.EveryWeekday && (day.DayOfWeek != DayOfWeek.Sunday &&
                day.DayOfWeek != DayOfWeek.Saturday))
                return DayForward(evnt, day);
            if (evnt.RecurringFrequency == RecurringFrequencies.Yearly && evnt.Date.Month == day.Month &&
                evnt.Date.Day == day.Day)
                return DayForward(evnt, day);
            if (evnt.RecurringFrequency == RecurringFrequencies.Monthly && evnt.Date.Day == day.Day)
                return DayForward(evnt, day);
            if (evnt.RecurringFrequency == RecurringFrequencies.Custom && evnt.CustomRecurringFunction != null)
            {
                if (evnt.CustomRecurringFunction(evnt, day))
                    return DayForward(evnt, day);
                return false;
            }

            
            if (evnt.RecurringFrequency == RecurringFrequencies.None && evnt.Date.Year == day.Year &&
                evnt.Date.Month == day.Month && evnt.Date.Day == day.Day)
                return DayForward(evnt, day);
            return false;
        }

        private int NumberOfWeeks(int year, int month)
        {
            return NumberOfWeeks(new DateTime(year, month, DateTime.DaysInMonth(year, month)));
        }

        private int NumberOfWeeks(DateTime date)
        {
            var beginningOfMonth = new DateTime(date.Year, date.Month, 1);

            while (date.Date.AddDays(1).DayOfWeek != CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek)
                date = date.AddDays(1);

            return (int)Math.Truncate(date.Subtract(beginningOfMonth).TotalDays / 7f) + 1;
        }

        private void CalendarResize(object sender, EventArgs e)
        {
            if (_calendarView == CalendarViews.Day)
                ResizeScrollPanel();
        }
    }

    #endregion

    #endregion

    #region 캘린더 내부의 ScrollPanel (수정 금지)
    public class ScrollPanel : UserControl
    {

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // ScrollPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.DoubleBuffered = true;
            this.Name = "ScrollPanel";
            this.Size = new System.Drawing.Size(327, 223);
            this.Load += new System.EventHandler(this.ScrollPanel_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.ScrollPanel_Paint);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ScrollPanel_MouseClick);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ScrollPanel_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ScrollPanel_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ScrollPanel_MouseUp);
            this.ResumeLayout(false);

        }


        private int _scrollOffset;
        private bool _mouseDown;
        private Point _oldMouseCoords;
        private int _bmpSize;

        public event MouseEventHandler RightButtonClicked;

        public ScrollPanel()
        {
            //InitializeComponent();
            _scrollOffset = 0;
            _mouseDown = false;
            _oldMouseCoords = new Point(0, 0);
        }

        public int ScrollOffset
        {
            get { return _scrollOffset; }
        }

        private void ScrollPanel_Load(object sender, EventArgs e)
        {
            //BackColor = Color.Blue;
        }

        // Paint
        private void ScrollPanel_Paint(object sender, PaintEventArgs e)
        {
            // Calendar 가져옴
            var c = (Calendar)Parent;

            Bitmap bmp = c.RequestImage();
            if (bmp == null)
                return;

            e.Graphics.DrawImage(bmp, new Rectangle(0, 0, ClientSize.Width, ClientSize.Height),
                                 new Rectangle(0, _scrollOffset, ClientSize.Width, ClientSize.Height),
                                 GraphicsUnit.Pixel);
            _bmpSize = bmp.Height;

            bmp.Dispose();
        }

        // mouse Down 이벤트
        private void ScrollPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _mouseDown = true;
                _oldMouseCoords = e.Location;
            }
        }

        private void ScrollPanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                _mouseDown = false;
        }

        private void ScrollPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseDown && e.Location.Y < _oldMouseCoords.Y && _scrollOffset < _bmpSize - _scrollOffset - ClientSize.Height)
            {
                int offset = _oldMouseCoords.Y - e.Location.Y;
                _scrollOffset += offset;
                Refresh();
            }
            if (_mouseDown && e.Location.Y > _oldMouseCoords.Y && _scrollOffset > 0)
            {
                int offset = e.Location.Y - _oldMouseCoords.Y;
                _scrollOffset -= offset;
                Refresh();
            }
            _oldMouseCoords = e.Location;
        }

        private void ScrollPanel_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (RightButtonClicked != null)
                    RightButtonClicked(sender, e);
            }
        }
    }

    #endregion

    #region CustomEvent 구조체 (수정 금지)
    public class CustomEvent : IEvent
    {
        public int Rank
        {
            get;
            set;
        }

        public float EventLengthInHours
        {
            get;
            set;
        }

        public bool Enabled
        {
            get;
            set;
        }

        public CustomRecurringFrequenciesHandler CustomRecurringFunction
        {
            get;
            set;
        }

        public bool IgnoreTimeComponent
        {
            get;
            set;
        }

        public bool ReadOnlyEvent
        {
            get;
            set;
        }

        public DateTime Date
        {
            get;
            set;
        }

        public Color EventColor
        {
            get;
            set;
        }

        public Font EventFont
        {
            get;
            set;
        }

        public string etc
        {
            get;
            set;
        }

        public string EventText
        {
            get;
            set;
        }

        public Color EventTextColor
        {
            get;
            set;
        }

        public RecurringFrequencies RecurringFrequency
        {
            get;
            set;
        }

        public bool TooltipEnabled
        {
            get;
            set;
        }

        public bool ThisDayForwardOnly
        {
            get;
            set;
        }

        public string startTime
        {
            get;


            set;
        }

        public string endTime
        {
            get;
            set;
        }

        /// <summary>
        /// CustomEvent Constructor
        /// </summary>
        public CustomEvent()
        {
            EventColor = Color.FromArgb(255, 255, 255);
            EventFont = new Font("Arial", 8, FontStyle.Bold);
            EventTextColor = Color.FromArgb(255, 255, 255);
            Rank = 2;
            EventLengthInHours = 1.0f;
            ReadOnlyEvent = false;
            Enabled = true;
            IgnoreTimeComponent = false;
            TooltipEnabled = true;
            ThisDayForwardOnly = true;
            RecurringFrequency = RecurringFrequencies.None;
        }

        public IEvent Clone()
        {
            return new CustomEvent
            {
                CustomRecurringFunction = CustomRecurringFunction,
                Date = Date,
                Enabled = Enabled,
                EventColor = EventColor,
                EventFont = EventFont,
                EventText = EventText,
                EventTextColor = EventTextColor,
                IgnoreTimeComponent = IgnoreTimeComponent,
                Rank = Rank,
                ReadOnlyEvent = ReadOnlyEvent,
                RecurringFrequency = RecurringFrequency,
                ThisDayForwardOnly = ThisDayForwardOnly,
                EventLengthInHours = EventLengthInHours,
                TooltipEnabled = TooltipEnabled
            };
        }
    }
    #endregion
}