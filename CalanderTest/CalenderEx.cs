using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CalanderTest
{

    public enum WorkType
    {
        Working,            // 근무 함
        NotWorking,         // 근무 안함
        incompleteWorking   // 부분 근무

    }

    public class WorkInfo
    {
        public WorkType workType;
        public int workTime;
        public string note;
    }

    public class CalenderEx : System.Windows.Forms.Panel
    {
        private DatePanel[] Dates;
        private DatePanel[] tempDates;

        private DateTime curDate; 
        
        private int ControlWidth;
        private int ControlHeight;
        private int Margin;

        private int DrawingX;
        private int DrawingY;
        private int DrawingDay;
        private int UpperMargin;

        int priorToFirstDays;
        float fontSize;

        public delegate void DateClick(DateTime date);
        public DateClick DateClickEvent;

        public CalenderEx() : base()
        {

            UpperMargin = 100;

            fontSize = 10;

            curDate = DateTime.Now;

            priorToFirstDays = 0;

            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);

            this.Resize += CalenderEx_Resize;
        }

        private void CalenderEx_Resize(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("resize 호출!!");
            
        }

        #region public 함수

        public void AddMonth()
        {
            Dates = null;
            curDate = curDate.AddMonths(1);
            Invalidate();
        }

        public void reduceMonth()
        {
            Dates = null;
            curDate = curDate.AddMonths(-1);
            Invalidate();
        }

        public void DateClickListener(DateTime dt)
        {
            DateClickEvent(dt);
        }
        #endregion


        #region 코어 함수

        protected override void OnPaint(PaintEventArgs e)
        {

            // 여기서 그림
            Debug.WriteLine("onPaint Called!");

            Controls.Clear();

            UpperMargin = Height / 5;
            fontSize = Height / 50;

            DrawingX = 0;
            DrawingY = UpperMargin;

            if (Dates == null)
            {
                Dates = new DatePanel[new DateTime(curDate.Year, curDate.Month, 1).AddMonths(1).AddDays(-1).Day];
            }

            int LineHeight = calculateHeight();

            ControlWidth = (Width ) / 7;
            ControlHeight = (Height - UpperMargin) / LineHeight;

            priorToFirstDays = CalculaterPriorFirstDay();       // 1일 이전의 날짜를 구함
            DrawingX = priorToFirstDays * ControlWidth;
            
            DrawYearMonthWeekString(e);       // 년도와 월 요일을 나타내는 String을 그립니다.
         
            // 날짜 칸 생성
            for (int i = 0;i < Dates.Length; i++)
            {
                if (Dates[i] == null)
                {
                    Dates[i] = new DatePanel(new DateTime(curDate.Year, curDate.Month, i + 1));
                }
                AddDate(Dates[i]);

            }
            base.OnPaint(e);
        }

        /// <summary>
        /// 날짜 박스 패널을 추가합니다.
        /// </summary>
        /// <param name="Date"></param>
        private void AddDate(DatePanel Date)
        {
            Date.DateClickEventHandler += DateClickListener;
            Date.Location = new Point(DrawingX, DrawingY);
            Date.Size = new Size(ControlWidth, ControlHeight);

            this.Controls.Add(Date);

            if (Date.dt.DayOfWeek == DayOfWeek.Saturday)
            {
                DrawingY += ControlHeight;
                DrawingX = 0;
            }
            else
            {
                DrawingX += ControlWidth;
            }
        }

        /// <summary>
        /// 년도와 날짜 요일 스트링을 그립니다.
        /// </summary>
        /// <param name="e"></param>
        private void DrawYearMonthWeekString(PaintEventArgs e)
        {
            string FontName = "새굴림";
            // 년도와 월 그리는 부분
            SizeF YearMonthFontSize = e.Graphics.MeasureString("YYYY년 MM월", new Font("Arial", fontSize * 2));
            e.Graphics.DrawString(curDate.Year.ToString() + "년 " + curDate.Month.ToString() + "월",
                new Font(FontName, fontSize * 2), new SolidBrush(Color.Black),
                new Point((Width / 2) - (int)(YearMonthFontSize.Width / 2), (UpperMargin / 2) - (int)(YearMonthFontSize.Height / 2)));

            // 요일 글씨 그리는 부분
            SizeF fontSz = e.Graphics.MeasureString("일", new Font("Arial", fontSize));
            e.Graphics.DrawString("일", new Font(FontName, fontSize), new SolidBrush(Color.Red),
                new Point(ControlWidth / 2 - (int)(fontSz.Width / 2), UpperMargin - (int)fontSz.Height - 10));
            e.Graphics.DrawString("월", new Font(FontName, fontSize), new SolidBrush(Color.Black),
                new Point(ControlWidth + (ControlWidth) / 2 - (int)(fontSz.Width / 2), UpperMargin - (int)fontSz.Height - 10));
            e.Graphics.DrawString("화", new Font(FontName, fontSize), new SolidBrush(Color.Black),
                new Point((ControlWidth * 2) + (ControlWidth) / 2 - (int)(fontSz.Width / 2), UpperMargin - (int)fontSz.Height - 10));
            e.Graphics.DrawString("수", new Font(FontName, fontSize), new SolidBrush(Color.Black),
                new Point((ControlWidth * 3) + (ControlWidth) / 2 - (int)(fontSz.Width / 2), UpperMargin - (int)fontSz.Height - 10));
            e.Graphics.DrawString("목", new Font(FontName, fontSize), new SolidBrush(Color.Black),
                new Point((ControlWidth * 4) + (ControlWidth) / 2 - (int)(fontSz.Width / 2), UpperMargin - (int)fontSz.Height - 10));
            e.Graphics.DrawString("금", new Font(FontName, fontSize), new SolidBrush(Color.Black),
                new Point((ControlWidth * 5) + (ControlWidth) / 2 - (int)(fontSz.Width / 2), UpperMargin - (int)fontSz.Height - 10));
            e.Graphics.DrawString("토", new Font(FontName, fontSize), new SolidBrush(Color.Red),
                new Point((ControlWidth * 6) + (ControlWidth) / 2 - (int)(fontSz.Width / 2), UpperMargin - (int)fontSz.Height - 10));
        }


        // 어떤 요일부터 날짜를 그려야 하는지 반환합니다.
        private int CalculaterPriorFirstDay()
        {
            // 현재 달의 1일
            DateTime dt = new DateTime(curDate.Year, curDate.Month, 1);

            switch(dt.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    return 1;
                case DayOfWeek.Tuesday:
                    return 2;
                case DayOfWeek.Wednesday:
                    return 3;
                case DayOfWeek.Thursday:
                    return 4;
                case DayOfWeek.Friday:
                    return 5;
                case DayOfWeek.Saturday:
                    return 6;
                case DayOfWeek.Sunday:
                    return 0;
                    
            }

            return 0;
        }

        // 해당달에 몇줄을 표시해야하는지 반환합니다.
        private int calculateHeight()
        {
            int count = 0;
            DateTime tempTime = new DateTime(curDate.Year, curDate.Month, 1);

            for (int i = 0; i < tempTime.AddMonths(1).AddDays(-1).Day; i++)
            {
                DateTime dt = new DateTime(curDate.Year, curDate.Month, i+1);

                if (dt.DayOfWeek == DayOfWeek.Saturday && tempTime.AddMonths(1).AddDays(-1).Day - 1 != i)
                    count++;
            }

            return count + 1;
        }

        public void selectClear(int date)
        {
            for(int i = 0;i < Controls.Count; i++)
            {
                if (((DatePanel)Controls[i]).dt.Day == date)
                    continue;
                if (((DatePanel)Controls[i]).isSelect == true)
                {
                    ((DatePanel)Controls[i]).isSelect = false;
                    Controls[i].Invalidate();
                }
            }
            
        }
        #endregion

    }

    #region DatePanel 부분

    class DatePanel : Panel
    {
        public DateTime dt;
        public delegate void DateClick(DateTime date);

        public bool isSelect;
        public bool haveData;

        public int workTime;
        public WorkType workType;
        private string workTypeString;
        public string note;

        // String을 그리기 위한 도구들
        Brush brush;
        Font font;
        Font stringFont;
        Pen pen;

        public DateClick DateClickEventHandler;

        public DatePanel(DateTime time) : base()
        {
            //System.Diagnostics.Debug.WriteLine(dt.Day +"일 생성자 호출됨");

            isSelect = false;
            haveData = false;

            this.dt = time;
            brush = new SolidBrush(Color.Black);
            font = new Font("새굴림", 12);
            stringFont = new Font("새굴림", 9);
            pen = new Pen(brush);

            workType = WorkType.NotWorking;
            workTime = 0;
            note = null;
            
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);

            this.Click += DatePanel_Click;
        }

        private void DatePanel_Click(object sender, EventArgs e)
        {
            if (!isSelect)
            {
                isSelect = true;
                ((CalenderEx)Parent).selectClear(dt.Day);
                Invalidate();
                return;
            }
            DateClickEventHandler(dt);
        }

        protected override void OnPaint(PaintEventArgs e)
        {

            if(dt.DayOfWeek == DayOfWeek.Sunday || dt.DayOfWeek == DayOfWeek.Saturday)
            {
                e.Graphics.DrawString(dt.Day.ToString(), font, new SolidBrush(Color.Red), new PointF(2, 2));
            }
            else
            {
                e.Graphics.DrawString(dt.Day.ToString(), font, brush, new PointF(2, 2));
            }
            if (isSelect)
            {
                this.BackColor = Color.SkyBlue;
            }
            else
            {
                this.BackColor = Color.White;
            }

            // 네모 테두리
            e.Graphics.DrawLine(pen, new Point(1,1), new Point(this.Width - 1,1));
            e.Graphics.DrawLine(pen, new Point(1, 1), new Point(1, this.Height -1));
            e.Graphics.DrawLine(pen, new Point(this.Width - 1, 1), new Point(this.Width - 1, this.Height -1));
            e.Graphics.DrawLine(pen, new Point(1,this.Height - 1), new Point(this.Width - 1, this.Height - 1));

            if (haveData)
            {
                // 날짜 내부의 String과 아이콘들 그림
                e.Graphics.FillRectangle(new SolidBrush(Color.Red), new Rectangle(new Point(Width / 4, Height / 4), new Size(5, 5)));
                e.Graphics.DrawString(workTypeString, font, brush, new Point((Width / 4) + 7, (Height / 4) - 5));

                pen.Color = Color.Blue;
                e.Graphics.FillRectangle(new SolidBrush(Color.Blue), new Rectangle(new Point(Width / 4, (Height / 4) * 2), new Size(5, 5)));
                e.Graphics.DrawString(workTime.ToString() + "시간",
                    font, brush, new Point((Width / 4) + 7, ((Height / 4) * 2) - 5));
                pen.Color = Color.Black;
                e.Graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(new Point(Width / 4, (Height / 4) * 3), new Size(5, 5)));
                e.Graphics.DrawString(note, font, brush, new Point((Width / 4) + 7, ((Height / 4) * 3) - 5));
            }
            //System.Diagnostics.Debug.WriteLine();
            base.OnPaint(e);
        }

        // 일을 추가함
        public void AddWork(WorkType type, int workTime, string note)
        {
            workType = type;
            setStringByWorkType(workType);

            this.workTime = workTime;
            this.note = note;

            haveData = true;

            Invalidate();
        }

        private void setStringByWorkType(WorkType workType)
        {
            switch(workType)
            {
                case WorkType.Working:

                    workTypeString = "근무";
                    break;
                case WorkType.NotWorking:
                    workTypeString = "미 근무";
                    break;
                case WorkType.incompleteWorking:
                    workTypeString = "부분 근무";
                    break;

            }
        }
    }

    #endregion 
}
