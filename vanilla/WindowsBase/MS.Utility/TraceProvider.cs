using System;
using System.Globalization;
using MS.Internal;

namespace MS.Utility;

internal abstract class TraceProvider
{
	protected bool _enabled;

	protected EventTrace.Level _level;

	protected EventTrace.Keyword _keywords;

	protected EventTrace.Keyword _matchAllKeyword;

	protected SecurityCriticalDataForSet<ulong> _registrationHandle;

	private const int s_basicTypeAllocationBufferSize = 16;

	private const int s_traceEventMaximumSize = 65482;

	private const int s_etwMaxNumberArguments = 32;

	private const int s_etwAPIMaxStringCount = 8;

	private const int ErrorEventTooBig = 2;

	internal EventTrace.Keyword Keywords => _keywords;

	internal EventTrace.Keyword MatchAllKeywords => _matchAllKeyword;

	internal EventTrace.Level Level => _level;

	internal TraceProvider()
	{
		_registrationHandle = new SecurityCriticalDataForSet<ulong>(0uL);
	}

	internal abstract void Register(Guid providerGuid);

	internal unsafe abstract uint EventWrite(EventTrace.Event eventID, EventTrace.Keyword keywords, EventTrace.Level level, int argc, EventData* argv);

	internal uint TraceEvent(EventTrace.Event eventID, EventTrace.Keyword keywords, EventTrace.Level level)
	{
		return TraceEvent(eventID, keywords, level, (object)null);
	}

	internal bool IsEnabled(EventTrace.Keyword keyword, EventTrace.Level level)
	{
		if (_enabled && (int)level <= (int)_level && (keyword & _keywords) != 0)
		{
			return (keyword & _matchAllKeyword) == _matchAllKeyword;
		}
		return false;
	}

	internal unsafe uint TraceEvent(EventTrace.Event eventID, EventTrace.Keyword keywords, EventTrace.Level level, object eventData)
	{
		uint num = 0u;
		int argc = 0;
		EventData eventData2 = default(EventData);
		eventData2.Size = 0u;
		string text = null;
		byte* dataBuffer = stackalloc byte[16];
		if (eventData != null)
		{
			text = EncodeObject(ref eventData, &eventData2, dataBuffer);
			argc = 1;
		}
		if (eventData2.Size > 65482)
		{
			return 2u;
		}
		if (text != null)
		{
			fixed (char* ptr = text)
			{
				eventData2.Ptr = (ulong)ptr;
				num = EventWrite(eventID, keywords, level, argc, &eventData2);
			}
		}
		else
		{
			num = EventWrite(eventID, keywords, level, argc, &eventData2);
		}
		return num;
	}

	internal unsafe uint TraceEvent(EventTrace.Event eventID, EventTrace.Keyword keywords, EventTrace.Level level, params object[] eventPayload)
	{
		//The blocks IL_00c2, IL_00c9, IL_00da, IL_00e3, IL_00e8, IL_00f1, IL_00f2, IL_00f9, IL_010a, IL_0113, IL_0118, IL_0121, IL_0122, IL_0129, IL_013a, IL_0143, IL_0148, IL_0151, IL_0152, IL_015e, IL_0174, IL_017a, IL_0190, IL_0196, IL_01ac, IL_01b2, IL_01c8, IL_01ce, IL_01e4, IL_01ea, IL_0200, IL_0206, IL_021c, IL_0222, IL_0238 are reachable both inside and outside the pinned region starting at IL_00bd. ILSpy has duplicated these blocks in order to place them both within and outside the `fixed` statement.
		//The blocks IL_00f2, IL_00f9, IL_010a, IL_0113, IL_0118, IL_0121, IL_0122, IL_0129, IL_013a, IL_0143, IL_0148, IL_0151, IL_0152, IL_015e, IL_0174, IL_017a, IL_0190, IL_0196, IL_01ac, IL_01b2, IL_01c8, IL_01ce, IL_01e4, IL_01ea, IL_0200, IL_0206, IL_021c, IL_0222, IL_0238 are reachable both inside and outside the pinned region starting at IL_00ed. ILSpy has duplicated these blocks in order to place them both within and outside the `fixed` statement.
		//The blocks IL_0122, IL_0129, IL_013a, IL_0143, IL_0148, IL_0151, IL_0152, IL_015e, IL_0174, IL_017a, IL_0190, IL_0196, IL_01ac, IL_01b2, IL_01c8, IL_01ce, IL_01e4, IL_01ea, IL_0200, IL_0206, IL_021c, IL_0222, IL_0238 are reachable both inside and outside the pinned region starting at IL_011d. ILSpy has duplicated these blocks in order to place them both within and outside the `fixed` statement.
		//The blocks IL_0152, IL_015e, IL_0174, IL_017a, IL_0190, IL_0196, IL_01ac, IL_01b2, IL_01c8, IL_01ce, IL_01e4, IL_01ea, IL_0200, IL_0206, IL_021c, IL_0222, IL_0238 are reachable both inside and outside the pinned region starting at IL_014d. ILSpy has duplicated these blocks in order to place them both within and outside the `fixed` statement.
		//The blocks IL_0152, IL_015e, IL_0174, IL_017a, IL_0190, IL_0196, IL_01ac, IL_01b2, IL_01c8, IL_01ce, IL_01e4, IL_01ea, IL_0200, IL_0206, IL_021c, IL_0222, IL_0238 are reachable both inside and outside the pinned region starting at IL_014d. ILSpy has duplicated these blocks in order to place them both within and outside the `fixed` statement.
		//The blocks IL_0122, IL_0129, IL_013a, IL_0143, IL_0148, IL_0151, IL_0152, IL_015e, IL_0174, IL_017a, IL_0190, IL_0196, IL_01ac, IL_01b2, IL_01c8, IL_01ce, IL_01e4, IL_01ea, IL_0200, IL_0206, IL_021c, IL_0222, IL_0238 are reachable both inside and outside the pinned region starting at IL_011d. ILSpy has duplicated these blocks in order to place them both within and outside the `fixed` statement.
		//The blocks IL_0152, IL_015e, IL_0174, IL_017a, IL_0190, IL_0196, IL_01ac, IL_01b2, IL_01c8, IL_01ce, IL_01e4, IL_01ea, IL_0200, IL_0206, IL_021c, IL_0222, IL_0238 are reachable both inside and outside the pinned region starting at IL_014d. ILSpy has duplicated these blocks in order to place them both within and outside the `fixed` statement.
		//The blocks IL_0152, IL_015e, IL_0174, IL_017a, IL_0190, IL_0196, IL_01ac, IL_01b2, IL_01c8, IL_01ce, IL_01e4, IL_01ea, IL_0200, IL_0206, IL_021c, IL_0222, IL_0238 are reachable both inside and outside the pinned region starting at IL_014d. ILSpy has duplicated these blocks in order to place them both within and outside the `fixed` statement.
		//The blocks IL_00f2, IL_00f9, IL_010a, IL_0113, IL_0118, IL_0121, IL_0122, IL_0129, IL_013a, IL_0143, IL_0148, IL_0151, IL_0152, IL_015e, IL_0174, IL_017a, IL_0190, IL_0196, IL_01ac, IL_01b2, IL_01c8, IL_01ce, IL_01e4, IL_01ea, IL_0200, IL_0206, IL_021c, IL_0222, IL_0238 are reachable both inside and outside the pinned region starting at IL_00ed. ILSpy has duplicated these blocks in order to place them both within and outside the `fixed` statement.
		//The blocks IL_0122, IL_0129, IL_013a, IL_0143, IL_0148, IL_0151, IL_0152, IL_015e, IL_0174, IL_017a, IL_0190, IL_0196, IL_01ac, IL_01b2, IL_01c8, IL_01ce, IL_01e4, IL_01ea, IL_0200, IL_0206, IL_021c, IL_0222, IL_0238 are reachable both inside and outside the pinned region starting at IL_011d. ILSpy has duplicated these blocks in order to place them both within and outside the `fixed` statement.
		//The blocks IL_0152, IL_015e, IL_0174, IL_017a, IL_0190, IL_0196, IL_01ac, IL_01b2, IL_01c8, IL_01ce, IL_01e4, IL_01ea, IL_0200, IL_0206, IL_021c, IL_0222, IL_0238 are reachable both inside and outside the pinned region starting at IL_014d. ILSpy has duplicated these blocks in order to place them both within and outside the `fixed` statement.
		//The blocks IL_0152, IL_015e, IL_0174, IL_017a, IL_0190, IL_0196, IL_01ac, IL_01b2, IL_01c8, IL_01ce, IL_01e4, IL_01ea, IL_0200, IL_0206, IL_021c, IL_0222, IL_0238 are reachable both inside and outside the pinned region starting at IL_014d. ILSpy has duplicated these blocks in order to place them both within and outside the `fixed` statement.
		//The blocks IL_0122, IL_0129, IL_013a, IL_0143, IL_0148, IL_0151, IL_0152, IL_015e, IL_0174, IL_017a, IL_0190, IL_0196, IL_01ac, IL_01b2, IL_01c8, IL_01ce, IL_01e4, IL_01ea, IL_0200, IL_0206, IL_021c, IL_0222, IL_0238 are reachable both inside and outside the pinned region starting at IL_011d. ILSpy has duplicated these blocks in order to place them both within and outside the `fixed` statement.
		//The blocks IL_0152, IL_015e, IL_0174, IL_017a, IL_0190, IL_0196, IL_01ac, IL_01b2, IL_01c8, IL_01ce, IL_01e4, IL_01ea, IL_0200, IL_0206, IL_021c, IL_0222, IL_0238 are reachable both inside and outside the pinned region starting at IL_014d. ILSpy has duplicated these blocks in order to place them both within and outside the `fixed` statement.
		//The blocks IL_0152, IL_015e, IL_0174, IL_017a, IL_0190, IL_0196, IL_01ac, IL_01b2, IL_01c8, IL_01ce, IL_01e4, IL_01ea, IL_0200, IL_0206, IL_021c, IL_0222, IL_0238 are reachable both inside and outside the pinned region starting at IL_014d. ILSpy has duplicated these blocks in order to place them both within and outside the `fixed` statement.
		int num = eventPayload.Length;
		uint num2 = 0u;
		int num3 = 0;
		int[] array = new int[8];
		string[] array2 = new string[8];
		EventData* ptr = stackalloc EventData[num];
		EventData* ptr2 = ptr;
		byte* ptr3 = stackalloc byte[(int)(uint)(16 * num)];
		for (int i = 0; i < num; i++)
		{
			if (eventPayload[i] != null)
			{
				string text = EncodeObject(ref eventPayload[i], ptr2, ptr3);
				ptr3 += 16;
				num2 = ptr2->Size;
				ptr2++;
				if (text != null)
				{
					array2[num3] = text;
					array[num3] = i;
					num3++;
				}
			}
		}
		if (num2 > 65482)
		{
			return 2u;
		}
		fixed (char* ptr18 = array2[0])
		{
			string obj = array2[1];
			char* intPtr;
			object obj2;
			object obj3;
			char* intPtr2;
			object obj4;
			object obj5;
			char* intPtr3;
			object obj6;
			object obj7;
			char* intPtr4;
			char* ptr5;
			if (obj != null)
			{
				fixed (char* ptr4 = &obj.GetPinnableReference())
				{
					intPtr = (ptr5 = ptr4);
					obj2 = array2[2];
					fixed (char* ptr6 = (string)obj2)
					{
						char* ptr7 = ptr6;
						obj3 = array2[3];
						char* ptr9;
						if (obj3 != null)
						{
							fixed (char* ptr8 = &((string)obj3).GetPinnableReference())
							{
								intPtr2 = (ptr9 = ptr8);
								obj4 = array2[4];
								fixed (char* ptr10 = (string)obj4)
								{
									char* ptr11 = ptr10;
									obj5 = array2[5];
									char* ptr13;
									if (obj5 != null)
									{
										fixed (char* ptr12 = &((string)obj5).GetPinnableReference())
										{
											intPtr3 = (ptr13 = ptr12);
											obj6 = array2[6];
											fixed (char* ptr14 = (string)obj6)
											{
												char* ptr15 = ptr14;
												obj7 = array2[7];
												char* ptr17;
												if (obj7 != null)
												{
													fixed (char* ptr16 = &((string)obj7).GetPinnableReference())
													{
														intPtr4 = (ptr17 = ptr16);
														ptr2 = ptr;
														if (array2[0] != null)
														{
															ptr2[array[0]].Ptr = (ulong)ptr18;
														}
														if (array2[1] != null)
														{
															ptr2[array[1]].Ptr = (ulong)ptr5;
														}
														if (array2[2] != null)
														{
															ptr2[array[2]].Ptr = (ulong)ptr7;
														}
														if (array2[3] != null)
														{
															ptr2[array[3]].Ptr = (ulong)ptr9;
														}
														if (array2[4] != null)
														{
															ptr2[array[4]].Ptr = (ulong)ptr11;
														}
														if (array2[5] != null)
														{
															ptr2[array[5]].Ptr = (ulong)ptr13;
														}
														if (array2[6] != null)
														{
															ptr2[array[6]].Ptr = (ulong)ptr15;
														}
														if (array2[7] != null)
														{
															ptr2[array[7]].Ptr = (ulong)ptr17;
														}
														return EventWrite(eventID, keywords, level, num, ptr);
													}
												}
												intPtr4 = (ptr17 = null);
												ptr2 = ptr;
												if (array2[0] != null)
												{
													ptr2[array[0]].Ptr = (ulong)ptr18;
												}
												if (array2[1] != null)
												{
													ptr2[array[1]].Ptr = (ulong)ptr5;
												}
												if (array2[2] != null)
												{
													ptr2[array[2]].Ptr = (ulong)ptr7;
												}
												if (array2[3] != null)
												{
													ptr2[array[3]].Ptr = (ulong)ptr9;
												}
												if (array2[4] != null)
												{
													ptr2[array[4]].Ptr = (ulong)ptr11;
												}
												if (array2[5] != null)
												{
													ptr2[array[5]].Ptr = (ulong)ptr13;
												}
												if (array2[6] != null)
												{
													ptr2[array[6]].Ptr = (ulong)ptr15;
												}
												if (array2[7] != null)
												{
													ptr2[array[7]].Ptr = (ulong)ptr17;
												}
												return EventWrite(eventID, keywords, level, num, ptr);
											}
										}
									}
									intPtr3 = (ptr13 = null);
									obj6 = array2[6];
									fixed (char* ptr14 = (string)obj6)
									{
										char* ptr15 = ptr14;
										obj7 = array2[7];
										char* ptr17;
										if (obj7 != null)
										{
											fixed (char* ptr16 = &((string)obj7).GetPinnableReference())
											{
												intPtr4 = (ptr17 = ptr16);
												ptr2 = ptr;
												if (array2[0] != null)
												{
													ptr2[array[0]].Ptr = (ulong)ptr18;
												}
												if (array2[1] != null)
												{
													ptr2[array[1]].Ptr = (ulong)ptr5;
												}
												if (array2[2] != null)
												{
													ptr2[array[2]].Ptr = (ulong)ptr7;
												}
												if (array2[3] != null)
												{
													ptr2[array[3]].Ptr = (ulong)ptr9;
												}
												if (array2[4] != null)
												{
													ptr2[array[4]].Ptr = (ulong)ptr11;
												}
												if (array2[5] != null)
												{
													ptr2[array[5]].Ptr = (ulong)ptr13;
												}
												if (array2[6] != null)
												{
													ptr2[array[6]].Ptr = (ulong)ptr15;
												}
												if (array2[7] != null)
												{
													ptr2[array[7]].Ptr = (ulong)ptr17;
												}
												return EventWrite(eventID, keywords, level, num, ptr);
											}
										}
										intPtr4 = (ptr17 = null);
										ptr2 = ptr;
										if (array2[0] != null)
										{
											ptr2[array[0]].Ptr = (ulong)ptr18;
										}
										if (array2[1] != null)
										{
											ptr2[array[1]].Ptr = (ulong)ptr5;
										}
										if (array2[2] != null)
										{
											ptr2[array[2]].Ptr = (ulong)ptr7;
										}
										if (array2[3] != null)
										{
											ptr2[array[3]].Ptr = (ulong)ptr9;
										}
										if (array2[4] != null)
										{
											ptr2[array[4]].Ptr = (ulong)ptr11;
										}
										if (array2[5] != null)
										{
											ptr2[array[5]].Ptr = (ulong)ptr13;
										}
										if (array2[6] != null)
										{
											ptr2[array[6]].Ptr = (ulong)ptr15;
										}
										if (array2[7] != null)
										{
											ptr2[array[7]].Ptr = (ulong)ptr17;
										}
										return EventWrite(eventID, keywords, level, num, ptr);
									}
								}
							}
						}
						intPtr2 = (ptr9 = null);
						obj4 = array2[4];
						fixed (char* ptr10 = (string)obj4)
						{
							char* ptr11 = ptr10;
							obj5 = array2[5];
							char* ptr13;
							if (obj5 != null)
							{
								fixed (char* ptr12 = &((string)obj5).GetPinnableReference())
								{
									intPtr3 = (ptr13 = ptr12);
									obj6 = array2[6];
									fixed (char* ptr14 = (string)obj6)
									{
										char* ptr15 = ptr14;
										obj7 = array2[7];
										char* ptr17;
										if (obj7 != null)
										{
											fixed (char* ptr16 = &((string)obj7).GetPinnableReference())
											{
												intPtr4 = (ptr17 = ptr16);
												ptr2 = ptr;
												if (array2[0] != null)
												{
													ptr2[array[0]].Ptr = (ulong)ptr18;
												}
												if (array2[1] != null)
												{
													ptr2[array[1]].Ptr = (ulong)ptr5;
												}
												if (array2[2] != null)
												{
													ptr2[array[2]].Ptr = (ulong)ptr7;
												}
												if (array2[3] != null)
												{
													ptr2[array[3]].Ptr = (ulong)ptr9;
												}
												if (array2[4] != null)
												{
													ptr2[array[4]].Ptr = (ulong)ptr11;
												}
												if (array2[5] != null)
												{
													ptr2[array[5]].Ptr = (ulong)ptr13;
												}
												if (array2[6] != null)
												{
													ptr2[array[6]].Ptr = (ulong)ptr15;
												}
												if (array2[7] != null)
												{
													ptr2[array[7]].Ptr = (ulong)ptr17;
												}
												return EventWrite(eventID, keywords, level, num, ptr);
											}
										}
										intPtr4 = (ptr17 = null);
										ptr2 = ptr;
										if (array2[0] != null)
										{
											ptr2[array[0]].Ptr = (ulong)ptr18;
										}
										if (array2[1] != null)
										{
											ptr2[array[1]].Ptr = (ulong)ptr5;
										}
										if (array2[2] != null)
										{
											ptr2[array[2]].Ptr = (ulong)ptr7;
										}
										if (array2[3] != null)
										{
											ptr2[array[3]].Ptr = (ulong)ptr9;
										}
										if (array2[4] != null)
										{
											ptr2[array[4]].Ptr = (ulong)ptr11;
										}
										if (array2[5] != null)
										{
											ptr2[array[5]].Ptr = (ulong)ptr13;
										}
										if (array2[6] != null)
										{
											ptr2[array[6]].Ptr = (ulong)ptr15;
										}
										if (array2[7] != null)
										{
											ptr2[array[7]].Ptr = (ulong)ptr17;
										}
										return EventWrite(eventID, keywords, level, num, ptr);
									}
								}
							}
							intPtr3 = (ptr13 = null);
							obj6 = array2[6];
							fixed (char* ptr14 = (string)obj6)
							{
								char* ptr15 = ptr14;
								obj7 = array2[7];
								char* ptr17;
								if (obj7 != null)
								{
									fixed (char* ptr16 = &((string)obj7).GetPinnableReference())
									{
										intPtr4 = (ptr17 = ptr16);
										ptr2 = ptr;
										if (array2[0] != null)
										{
											ptr2[array[0]].Ptr = (ulong)ptr18;
										}
										if (array2[1] != null)
										{
											ptr2[array[1]].Ptr = (ulong)ptr5;
										}
										if (array2[2] != null)
										{
											ptr2[array[2]].Ptr = (ulong)ptr7;
										}
										if (array2[3] != null)
										{
											ptr2[array[3]].Ptr = (ulong)ptr9;
										}
										if (array2[4] != null)
										{
											ptr2[array[4]].Ptr = (ulong)ptr11;
										}
										if (array2[5] != null)
										{
											ptr2[array[5]].Ptr = (ulong)ptr13;
										}
										if (array2[6] != null)
										{
											ptr2[array[6]].Ptr = (ulong)ptr15;
										}
										if (array2[7] != null)
										{
											ptr2[array[7]].Ptr = (ulong)ptr17;
										}
										return EventWrite(eventID, keywords, level, num, ptr);
									}
								}
								intPtr4 = (ptr17 = null);
								ptr2 = ptr;
								if (array2[0] != null)
								{
									ptr2[array[0]].Ptr = (ulong)ptr18;
								}
								if (array2[1] != null)
								{
									ptr2[array[1]].Ptr = (ulong)ptr5;
								}
								if (array2[2] != null)
								{
									ptr2[array[2]].Ptr = (ulong)ptr7;
								}
								if (array2[3] != null)
								{
									ptr2[array[3]].Ptr = (ulong)ptr9;
								}
								if (array2[4] != null)
								{
									ptr2[array[4]].Ptr = (ulong)ptr11;
								}
								if (array2[5] != null)
								{
									ptr2[array[5]].Ptr = (ulong)ptr13;
								}
								if (array2[6] != null)
								{
									ptr2[array[6]].Ptr = (ulong)ptr15;
								}
								if (array2[7] != null)
								{
									ptr2[array[7]].Ptr = (ulong)ptr17;
								}
								return EventWrite(eventID, keywords, level, num, ptr);
							}
						}
					}
				}
			}
			intPtr = (ptr5 = null);
			obj2 = array2[2];
			fixed (char* ptr6 = (string)obj2)
			{
				char* ptr7 = ptr6;
				obj3 = array2[3];
				char* ptr9;
				if (obj3 != null)
				{
					fixed (char* ptr8 = &((string)obj3).GetPinnableReference())
					{
						intPtr2 = (ptr9 = ptr8);
						obj4 = array2[4];
						fixed (char* ptr10 = (string)obj4)
						{
							char* ptr11 = ptr10;
							obj5 = array2[5];
							char* ptr13;
							if (obj5 != null)
							{
								fixed (char* ptr12 = &((string)obj5).GetPinnableReference())
								{
									intPtr3 = (ptr13 = ptr12);
									obj6 = array2[6];
									fixed (char* ptr14 = (string)obj6)
									{
										char* ptr15 = ptr14;
										obj7 = array2[7];
										char* ptr17;
										if (obj7 != null)
										{
											fixed (char* ptr16 = &((string)obj7).GetPinnableReference())
											{
												intPtr4 = (ptr17 = ptr16);
												ptr2 = ptr;
												if (array2[0] != null)
												{
													ptr2[array[0]].Ptr = (ulong)ptr18;
												}
												if (array2[1] != null)
												{
													ptr2[array[1]].Ptr = (ulong)ptr5;
												}
												if (array2[2] != null)
												{
													ptr2[array[2]].Ptr = (ulong)ptr7;
												}
												if (array2[3] != null)
												{
													ptr2[array[3]].Ptr = (ulong)ptr9;
												}
												if (array2[4] != null)
												{
													ptr2[array[4]].Ptr = (ulong)ptr11;
												}
												if (array2[5] != null)
												{
													ptr2[array[5]].Ptr = (ulong)ptr13;
												}
												if (array2[6] != null)
												{
													ptr2[array[6]].Ptr = (ulong)ptr15;
												}
												if (array2[7] != null)
												{
													ptr2[array[7]].Ptr = (ulong)ptr17;
												}
												return EventWrite(eventID, keywords, level, num, ptr);
											}
										}
										intPtr4 = (ptr17 = null);
										ptr2 = ptr;
										if (array2[0] != null)
										{
											ptr2[array[0]].Ptr = (ulong)ptr18;
										}
										if (array2[1] != null)
										{
											ptr2[array[1]].Ptr = (ulong)ptr5;
										}
										if (array2[2] != null)
										{
											ptr2[array[2]].Ptr = (ulong)ptr7;
										}
										if (array2[3] != null)
										{
											ptr2[array[3]].Ptr = (ulong)ptr9;
										}
										if (array2[4] != null)
										{
											ptr2[array[4]].Ptr = (ulong)ptr11;
										}
										if (array2[5] != null)
										{
											ptr2[array[5]].Ptr = (ulong)ptr13;
										}
										if (array2[6] != null)
										{
											ptr2[array[6]].Ptr = (ulong)ptr15;
										}
										if (array2[7] != null)
										{
											ptr2[array[7]].Ptr = (ulong)ptr17;
										}
										return EventWrite(eventID, keywords, level, num, ptr);
									}
								}
							}
							intPtr3 = (ptr13 = null);
							obj6 = array2[6];
							fixed (char* ptr14 = (string)obj6)
							{
								char* ptr15 = ptr14;
								obj7 = array2[7];
								char* ptr17;
								if (obj7 != null)
								{
									fixed (char* ptr16 = &((string)obj7).GetPinnableReference())
									{
										intPtr4 = (ptr17 = ptr16);
										ptr2 = ptr;
										if (array2[0] != null)
										{
											ptr2[array[0]].Ptr = (ulong)ptr18;
										}
										if (array2[1] != null)
										{
											ptr2[array[1]].Ptr = (ulong)ptr5;
										}
										if (array2[2] != null)
										{
											ptr2[array[2]].Ptr = (ulong)ptr7;
										}
										if (array2[3] != null)
										{
											ptr2[array[3]].Ptr = (ulong)ptr9;
										}
										if (array2[4] != null)
										{
											ptr2[array[4]].Ptr = (ulong)ptr11;
										}
										if (array2[5] != null)
										{
											ptr2[array[5]].Ptr = (ulong)ptr13;
										}
										if (array2[6] != null)
										{
											ptr2[array[6]].Ptr = (ulong)ptr15;
										}
										if (array2[7] != null)
										{
											ptr2[array[7]].Ptr = (ulong)ptr17;
										}
										return EventWrite(eventID, keywords, level, num, ptr);
									}
								}
								intPtr4 = (ptr17 = null);
								ptr2 = ptr;
								if (array2[0] != null)
								{
									ptr2[array[0]].Ptr = (ulong)ptr18;
								}
								if (array2[1] != null)
								{
									ptr2[array[1]].Ptr = (ulong)ptr5;
								}
								if (array2[2] != null)
								{
									ptr2[array[2]].Ptr = (ulong)ptr7;
								}
								if (array2[3] != null)
								{
									ptr2[array[3]].Ptr = (ulong)ptr9;
								}
								if (array2[4] != null)
								{
									ptr2[array[4]].Ptr = (ulong)ptr11;
								}
								if (array2[5] != null)
								{
									ptr2[array[5]].Ptr = (ulong)ptr13;
								}
								if (array2[6] != null)
								{
									ptr2[array[6]].Ptr = (ulong)ptr15;
								}
								if (array2[7] != null)
								{
									ptr2[array[7]].Ptr = (ulong)ptr17;
								}
								return EventWrite(eventID, keywords, level, num, ptr);
							}
						}
					}
				}
				intPtr2 = (ptr9 = null);
				obj4 = array2[4];
				fixed (char* ptr10 = (string)obj4)
				{
					char* ptr11 = ptr10;
					obj5 = array2[5];
					char* ptr13;
					if (obj5 != null)
					{
						fixed (char* ptr12 = &((string)obj5).GetPinnableReference())
						{
							intPtr3 = (ptr13 = ptr12);
							obj6 = array2[6];
							fixed (char* ptr14 = (string)obj6)
							{
								char* ptr15 = ptr14;
								obj7 = array2[7];
								char* ptr17;
								if (obj7 != null)
								{
									fixed (char* ptr16 = &((string)obj7).GetPinnableReference())
									{
										intPtr4 = (ptr17 = ptr16);
										ptr2 = ptr;
										if (array2[0] != null)
										{
											ptr2[array[0]].Ptr = (ulong)ptr18;
										}
										if (array2[1] != null)
										{
											ptr2[array[1]].Ptr = (ulong)ptr5;
										}
										if (array2[2] != null)
										{
											ptr2[array[2]].Ptr = (ulong)ptr7;
										}
										if (array2[3] != null)
										{
											ptr2[array[3]].Ptr = (ulong)ptr9;
										}
										if (array2[4] != null)
										{
											ptr2[array[4]].Ptr = (ulong)ptr11;
										}
										if (array2[5] != null)
										{
											ptr2[array[5]].Ptr = (ulong)ptr13;
										}
										if (array2[6] != null)
										{
											ptr2[array[6]].Ptr = (ulong)ptr15;
										}
										if (array2[7] != null)
										{
											ptr2[array[7]].Ptr = (ulong)ptr17;
										}
										return EventWrite(eventID, keywords, level, num, ptr);
									}
								}
								intPtr4 = (ptr17 = null);
								ptr2 = ptr;
								if (array2[0] != null)
								{
									ptr2[array[0]].Ptr = (ulong)ptr18;
								}
								if (array2[1] != null)
								{
									ptr2[array[1]].Ptr = (ulong)ptr5;
								}
								if (array2[2] != null)
								{
									ptr2[array[2]].Ptr = (ulong)ptr7;
								}
								if (array2[3] != null)
								{
									ptr2[array[3]].Ptr = (ulong)ptr9;
								}
								if (array2[4] != null)
								{
									ptr2[array[4]].Ptr = (ulong)ptr11;
								}
								if (array2[5] != null)
								{
									ptr2[array[5]].Ptr = (ulong)ptr13;
								}
								if (array2[6] != null)
								{
									ptr2[array[6]].Ptr = (ulong)ptr15;
								}
								if (array2[7] != null)
								{
									ptr2[array[7]].Ptr = (ulong)ptr17;
								}
								return EventWrite(eventID, keywords, level, num, ptr);
							}
						}
					}
					intPtr3 = (ptr13 = null);
					obj6 = array2[6];
					fixed (char* ptr14 = (string)obj6)
					{
						char* ptr15 = ptr14;
						obj7 = array2[7];
						char* ptr17;
						if (obj7 != null)
						{
							fixed (char* ptr16 = &((string)obj7).GetPinnableReference())
							{
								intPtr4 = (ptr17 = ptr16);
								ptr2 = ptr;
								if (array2[0] != null)
								{
									ptr2[array[0]].Ptr = (ulong)ptr18;
								}
								if (array2[1] != null)
								{
									ptr2[array[1]].Ptr = (ulong)ptr5;
								}
								if (array2[2] != null)
								{
									ptr2[array[2]].Ptr = (ulong)ptr7;
								}
								if (array2[3] != null)
								{
									ptr2[array[3]].Ptr = (ulong)ptr9;
								}
								if (array2[4] != null)
								{
									ptr2[array[4]].Ptr = (ulong)ptr11;
								}
								if (array2[5] != null)
								{
									ptr2[array[5]].Ptr = (ulong)ptr13;
								}
								if (array2[6] != null)
								{
									ptr2[array[6]].Ptr = (ulong)ptr15;
								}
								if (array2[7] != null)
								{
									ptr2[array[7]].Ptr = (ulong)ptr17;
								}
								return EventWrite(eventID, keywords, level, num, ptr);
							}
						}
						intPtr4 = (ptr17 = null);
						ptr2 = ptr;
						if (array2[0] != null)
						{
							ptr2[array[0]].Ptr = (ulong)ptr18;
						}
						if (array2[1] != null)
						{
							ptr2[array[1]].Ptr = (ulong)ptr5;
						}
						if (array2[2] != null)
						{
							ptr2[array[2]].Ptr = (ulong)ptr7;
						}
						if (array2[3] != null)
						{
							ptr2[array[3]].Ptr = (ulong)ptr9;
						}
						if (array2[4] != null)
						{
							ptr2[array[4]].Ptr = (ulong)ptr11;
						}
						if (array2[5] != null)
						{
							ptr2[array[5]].Ptr = (ulong)ptr13;
						}
						if (array2[6] != null)
						{
							ptr2[array[6]].Ptr = (ulong)ptr15;
						}
						if (array2[7] != null)
						{
							ptr2[array[7]].Ptr = (ulong)ptr17;
						}
						return EventWrite(eventID, keywords, level, num, ptr);
					}
				}
			}
		}
	}

	private unsafe static string EncodeObject(ref object data, EventData* dataDescriptor, byte* dataBuffer)
	{
		dataDescriptor->Reserved = 0u;
		if (data is string text)
		{
			dataDescriptor->Size = (uint)((text.Length + 1) * 2);
			return text;
		}
		Type type = data.GetType();
		if (type.IsEnum)
		{
			data = Convert.ChangeType(data, Enum.GetUnderlyingType(type), CultureInfo.InvariantCulture);
		}
		if (data is nint)
		{
			dataDescriptor->Size = (uint)sizeof(nint);
			*(nint*)dataBuffer = (nint)data;
			dataDescriptor->Ptr = (ulong)dataBuffer;
		}
		else if (data is int)
		{
			dataDescriptor->Size = 4u;
			*(int*)dataBuffer = (int)data;
			dataDescriptor->Ptr = (ulong)dataBuffer;
		}
		else if (data is long)
		{
			dataDescriptor->Size = 8u;
			*(long*)dataBuffer = (long)data;
			dataDescriptor->Ptr = (ulong)dataBuffer;
		}
		else if (data is uint)
		{
			dataDescriptor->Size = 4u;
			*(uint*)dataBuffer = (uint)data;
			dataDescriptor->Ptr = (ulong)dataBuffer;
		}
		else if (data is ulong)
		{
			dataDescriptor->Size = 8u;
			*(ulong*)dataBuffer = (ulong)data;
			dataDescriptor->Ptr = (ulong)dataBuffer;
		}
		else if (data is char)
		{
			dataDescriptor->Size = 2u;
			*(char*)dataBuffer = (char)data;
			dataDescriptor->Ptr = (ulong)dataBuffer;
		}
		else if (data is byte)
		{
			dataDescriptor->Size = 1u;
			*dataBuffer = (byte)data;
			dataDescriptor->Ptr = (ulong)dataBuffer;
		}
		else if (data is short)
		{
			dataDescriptor->Size = 2u;
			*(short*)dataBuffer = (short)data;
			dataDescriptor->Ptr = (ulong)dataBuffer;
		}
		else if (data is sbyte)
		{
			dataDescriptor->Size = 1u;
			*dataBuffer = (byte)(sbyte)data;
			dataDescriptor->Ptr = (ulong)dataBuffer;
		}
		else if (data is ushort)
		{
			dataDescriptor->Size = 2u;
			*(ushort*)dataBuffer = (ushort)data;
			dataDescriptor->Ptr = (ulong)dataBuffer;
		}
		else if (data is float)
		{
			dataDescriptor->Size = 4u;
			*(float*)dataBuffer = (float)data;
			dataDescriptor->Ptr = (ulong)dataBuffer;
		}
		else if (data is double)
		{
			dataDescriptor->Size = 8u;
			*(double*)dataBuffer = (double)data;
			dataDescriptor->Ptr = (ulong)dataBuffer;
		}
		else if (data is bool)
		{
			dataDescriptor->Size = 1u;
			*dataBuffer = (((bool)data) ? ((byte)1) : ((byte)0));
			dataDescriptor->Ptr = (ulong)dataBuffer;
		}
		else if (data is Guid)
		{
			dataDescriptor->Size = (uint)sizeof(Guid);
			*(Guid*)dataBuffer = (Guid)data;
			dataDescriptor->Ptr = (ulong)dataBuffer;
		}
		else if (data is decimal)
		{
			dataDescriptor->Size = 16u;
			*(decimal*)dataBuffer = (decimal)data;
			dataDescriptor->Ptr = (ulong)dataBuffer;
		}
		else
		{
			if (!(data is bool))
			{
				string text2 = data.ToString();
				dataDescriptor->Size = (uint)((text2.Length + 1) * 2);
				return text2;
			}
			dataDescriptor->Size = 1u;
			*dataBuffer = (((bool)data) ? ((byte)1) : ((byte)0));
			dataDescriptor->Ptr = (ulong)dataBuffer;
		}
		return null;
	}
}
