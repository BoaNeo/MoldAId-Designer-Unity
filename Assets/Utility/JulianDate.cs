using System;

namespace Utility {
	public class JulianDate {
		public static int YEAR = 0;
		public static int MONTH = 1;
		public static int DAY_OF_MONTH = 2;
		public static int HOUR_OF_DAY = 3;
		public static int MINUTE = 4;
		public static int SECOND = 5;
		public static int NUMFIELDS = 6;

		public static double MIN_DATE = 0.0;
		public static double MAX_DATE = 5373484.5;

		double m_fJD;

		/**
		 Constructs a JulianDate initialized with the current date and time
		 */
		/*
		public JulianDate()
		{
		  set(now());
		}
		*/
		/**
	   *   Constructs a JulianDate initialized with the Julian Day parameter
	   *   @param jd the Julian Day, which is the number of days since noon at January 1 of the year -4712
	   */
		public JulianDate(double jd) {
			set(jd);
		}

		/**
	   * Constructs a JulianDate initialized with the given date and time
	   *
	   * @param year -4713 to 9999
	   * @param month 1 to 12
	   * @param day 1 to 31
	   * @param hour 0 to 23
	   * @param minute 0 to 59
	   * @param second 0 to 59
	   */
		public JulianDate(int year, int month, int day, int hour, int minute, int second) {
			set(year, month, day, hour, minute, second);
		}

		/**
	   * Constructs a JulianDate initialized with the given date and time
	   */
		public JulianDate(int[] dt) {
			set(dt);
		}

		public JulianDate(DateTime ts) {
			set(encode(ts));
			//    Calendar c=Calendar.getInstance();
			//  c.setTime(ts);
			//set(encodeJD(c.get(Calendar.YEAR), c.get(Calendar.MONTH)+1, c.get(Calendar.DAY_OF_MONTH), c.get(Calendar.HOUR_OF_DAY), c.get(Calendar.MINUTE), c.get(Calendar.SECOND)));
		}

		/**
	   * Returns the Julian Day representation of a date/time
	   * @return double in the range from 0 to 5373484.5 as number of days since -4713-11-24 12:00:00
	   */
		public double get() {
			return m_fJD;
		}

		/**
	   * Gets the date/time elements as an array of integers. The contents of the array represents year, month, day and
	   * so forth. (You may wish to use the public field IDs defined in this class to index the array properly).
	   *
	   * @param buffer Buffer to use or null if this method should allocate a new buffer. Note that a new buffer WILL be
	   *               allocated if the supplied buffer is too small (has less than 6 elements).
	   *
	   * @return Date/time parts of this DateTime object
	   */
		public int[] get(int[] buffer) {
			return decodeJD(m_fJD, buffer);
		}

		public bool isAfter(double fDate) {
			return m_fJD > fDate;
		}

		public bool isBefore(double fDate) {
			return m_fJD < fDate;
		}

		/**
	   Sets the Julian Day of a date/time
	   @param jd the number of days since-4713-11-24 12:00:00 in the range from 0 to 5373484.5
	   */
		public void set(double jd) {
			m_fJD = jd;
		}

		/**
	   * Set the DateTime object to a specific date
	   *
	   * @param year -4713 to 9999
	   * @param month 1 to 12
	   * @param day 1 to 31
	   * @param hour 0 to 23
	   * @param minute 0 to 59
	   * @param second 0 to 59
	   */
		public void set(int year, int month, int day, int hour, int minute, int second) {
			set(encodeJD(year, month, day, hour, minute, second));
		}

		/**
	   * Set the DateTime object to a specific date
	   *
	   */
		public void set(int[] dt) {
			set(dt[YEAR], dt[MONTH], dt[DAY_OF_MONTH], dt[HOUR_OF_DAY], dt[MINUTE], dt[SECOND]);
		}

		/**
	   * Add time to this DateTime object
	   *
	   * @param years
	   * @param months
	   * @param days
	   * @param hours
	   * @param minutes
	   * @param seconds
	   */
		public void add(int years, int months, int days, int hours, int minutes, int seconds) {
			int[] dt = get(null);
			dt[YEAR] += years;
			dt[MONTH] += months;
			dt[DAY_OF_MONTH] += days;
			dt[HOUR_OF_DAY] += hours;
			dt[MINUTE] += minutes;
			dt[SECOND] += seconds;
			set(dt);
		}

		public static int getLastDayOfMonth(int year, int month) {
			switch (month) {
				case 1:
				case 3:
				case 5:
				case 7:
				case 8:
				case 10:
				case 12:
					return 31;
				case 4:
				case 6:
				case 9:
				case 11:
					return 30;
				case 2:
					int[] dt = new int[6];

					// Adjust the day to match the last day of the month of that year:
					dt[YEAR] = year;
					dt[MONTH] = month;
					dt[DAY_OF_MONTH] = 32;
					do {
						dt[DAY_OF_MONTH]--;
					} while (!IsValid(dt[YEAR], dt[MONTH], dt[DAY_OF_MONTH], dt[HOUR_OF_DAY], dt[MINUTE], dt[SECOND]));

					return dt[DAY_OF_MONTH];
			}

			return -1;
		}

		/**
		 Returns a textual representation of a date/time object
		 @return String
		 */
		/*  
		  public String toSQL()
		  {
		    int[] dt=get(null);
		    
		    StringBuilder b=new StringBuffer(20);
		    
		    if(dt[YEAR]<0)
		    {
		      b.append("-");
		      dt[YEAR]=-dt[YEAR];
		    }
		    else
		      b.append(" ");
		    
		    NumberFormat nf=NumberFormat.getInstance();
		    nf.setGroupingUsed(false);
		    nf.setMaximumIntegerDigits(4);
		    nf.setMinimumIntegerDigits(4);
		    b.append(nf.format(dt[YEAR]));
		    nf.setMaximumIntegerDigits(2);
		    nf.setMinimumIntegerDigits(2);
		    b.append("-");
		    b.append(nf.format(dt[MONTH]));
		    b.append("-");
		    b.append(nf.format(dt[DAY_OF_MONTH]));
		    b.append(" ");
		    b.append(nf.format(dt[HOUR_OF_DAY]));
		    b.append(":");
		    b.append(nf.format(dt[MINUTE]));
		    b.append(":");
		    b.append(nf.format(dt[SECOND]));
		    
		    return b.toString();
		  }
	
		  public String toString()
		  {
		    return toSQL();
		  }
	
		  public String toString(TimeZone tz)
		  {
		    double offset = tz.getOffset(System.currentTimeMillis())/((double)1000*60*60*24);
		    
		    
		    JulianDate d = new JulianDate(get()+offset);
		    return d.toSQL();
		  }
		*/
		/**
	   Determines whether a date/time is valid taking the different number of days per month and leap years into account
	   @param Year -4713 to 9999
	   @param Month 1 to 12
	   @param Day 1 to 31
	   @param Hour 0 to 23
	   @param Minute 0 to 59
	   @param Second 0 to 59
	   @return boolean indicating whether the date/time is valid
	   */
		public static bool IsValid(int Year, int Month, int Day, int Hour, int Minute, int Second) {
			try {
				JulianDate d = new JulianDate(Year, Month, Day, Hour, Minute, Second);

				int[] dt = d.get(null);
				if (Year != dt[YEAR])
					return false;
				if (Month != dt[MONTH])
					return false;
				if (Day != dt[DAY_OF_MONTH])
					return false;
				if (Hour != dt[HOUR_OF_DAY])
					return false;
				if (Minute != dt[MINUTE])
					return false;
				if (Second != dt[SECOND])
					return false;
			}
			catch {
				return false;
			}

			return true;
		}

		public static int[] decodeJD(double fJD, int[] buffer) {
			if (buffer == null || buffer.Length < NUMFIELDS)
				buffer = new int[6];


			double F = Math.Abs(fJD - (long) fJD); // Fraction part of day


			long L = ((long) fJD) + 68569;
			if (F >= 0.5) // Relative to noon !!!
			{
				L++;
				F -= 1.0;
			}

			long N = ((4 * L) / 146097);
			L = L - (((146097 * N + 3) / 4));
			long I = ((4000 * (L + 1) / 1461001));
			L = L - ((1461 * I) / 4) + 31;
			long J = ((80 * L) / 2447);
			buffer[DAY_OF_MONTH] = (int) (L - ((2447 * J) / 80));
			L = (J / 11);
			buffer[MONTH] = (int) (J + 2 - 12 * L);
			buffer[YEAR] = (int) (100 * (N - 49) + I + L);

			double secs = F * 86400.0;

			buffer[HOUR_OF_DAY] = (int) Math.Floor(secs / 3600.0);
			secs -= buffer[HOUR_OF_DAY] * 3600;

			buffer[MINUTE] = (int) Math.Floor(secs / 60.0);
			secs -= buffer[MINUTE] * 60;

			buffer[SECOND] = (int) Math.Round(secs);

			buffer[HOUR_OF_DAY] += 12;

			if (buffer[SECOND] == 60) {
				buffer[SECOND] = 0;
				buffer[MINUTE]++;
				if (buffer[MINUTE] == 60) {
					buffer[MINUTE] = 0;
					buffer[HOUR_OF_DAY]++;
					if (buffer[HOUR_OF_DAY] == 24) {
						buffer[HOUR_OF_DAY] = 0;
						buffer[DAY_OF_MONTH]++;
						if (buffer[DAY_OF_MONTH] > JulianDate.getLastDayOfMonth(buffer[YEAR], buffer[MONTH])) {
							buffer[DAY_OF_MONTH] = 1;
							buffer[MONTH]++;
							if (buffer[MONTH] == 12) {
								buffer[MONTH] = 1;
								buffer[YEAR]++;
							}
						}
					}
				}
			}

			return buffer;
		}

		public static double encodeJD(int year, int month, int day, int hour, int minute, int second) {
			int y = year, m = month, d = day;

			if (m < 3) {
				m = m + 12;
				y = y - 1;
			}

			long jd = d + (153 * m - 457) / 5 + 365 * y + (long) Math.Floor((double) y / 4) - (long) Math.Floor((double) y / 100) + (long) Math.Floor((double) y / 400) + 1721119;

			double secs = (second + 60 * minute + 60 * 60 * (hour - 12)) / 86400.0;

		//	secs = Math.Round(secs * 1000000.0) / 1000000.0; // Prevent arbitrary round-off errors on different databases by always rounding on the 6th decimal
			return (jd + (secs));
		}
		/*
		public static double now()
		{
		  Calendar c=Calendar.getInstance(TimeZone.getTimeZone("GMT"));
		  return encode(c);
		}
		*/
		/*  
		  public static double encode(Calendar c)
		  {
		    return encodeJD(c.get(Calendar.YEAR), c.get(Calendar.MONTH)+1, c.get(Calendar.DAY_OF_MONTH), c.get(Calendar.HOUR_OF_DAY), c.get(Calendar.MINUTE), c.get(Calendar.SECOND));
		  }
		  */

		public static double encode(DateTime dt) {
			return encodeJD(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
		}

		public static double daysOnly(double jd) {
			return Math.Floor(jd + .5);
		}

		public static double zeroHour(double jd) {
			return Math.Round(jd) - .5;
		}

		public static DateTime toDateTime(double jd) {
			int[] buf = decodeJD(jd, null);
			return new DateTime(buf[0], buf[1], buf[2], buf[3], buf[4], buf[5]);
		}

		public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public static DateTime FromMillisecondsSinceUnixEpoch(long milliseconds) {
			return UnixEpoch.AddMilliseconds(milliseconds);
		}
	}
}