import {
  attendanceViewState,
  elapsedWholeSeconds,
  formatCountdown,
  formatServerClock,
  isClosingSoon,
  parseServerClock,
  secondsRemainingAt,
  serverClockAt,
} from "./studentAttendance";

describe("student meal attendance view state", () => {
  it("does not start the countdown before the server-authoritative opening time", () => {
    expect(
      attendanceViewState({
        periodoAbierto: false,
        periodoCerrado: false,
        segundosParaApertura: 3600,
      }),
    ).toEqual("pending");
  });

  it("shows an active countdown only during the confirmation window", () => {
    expect(
      attendanceViewState({ periodoAbierto: true, periodoCerrado: false, segundosParaCierre: 65 }),
    ).toEqual("open");
  });

  it("formats time remaining as hours, minutes and seconds so it visibly advances each second", () => {
    expect(formatCountdown(65)).toBe("00 h 01 min 05 s");
    expect(formatCountdown(3601)).toBe("01 h 00 min 01 s");
    expect(formatCountdown(0)).toBe("00 h 00 min 00 s");
    expect(formatCountdown(null)).toBeNull();
  });

  it("parses and advances the server clock without using the browser timezone", () => {
    expect(parseServerClock("10:05:07")).toBe(36_307);
    expect(parseServerClock("24:00:00")).toBeNull();
    expect(formatServerClock(36_308)).toBe("10:05:08");
    expect(formatServerClock(86_400)).toBe("00:00:00");
    expect(formatServerClock(null)).toBeNull();
  });

  it("derives elapsed time from the synchronized timestamp instead of interval ticks", () => {
    const synchronizedAt = 1_000;
    const delayedRenderAt = 5_800;

    expect(elapsedWholeSeconds(synchronizedAt, delayedRenderAt)).toBe(4);
    expect(secondsRemainingAt(30, synchronizedAt, delayedRenderAt)).toBe(26);
    expect(serverClockAt(36_000, synchronizedAt, delayedRenderAt)).toBe(36_004);
  });

  it("uses the configured warning threshold instead of a fixed fifteen minutes", () => {
    expect(isClosingSoon(10 * 60, 15)).toBe(true);
    expect(isClosingSoon(10 * 60, 5)).toBe(false);
    expect(isClosingSoon(0, 15)).toBe(false);
  });

  it("turns a confirmed attendance into a final, non-interactive state after closure", () => {
    expect(
      attendanceViewState({ periodoAbierto: true, periodoCerrado: true, estado: "Confirmada" }),
    ).toEqual("expired-confirmed");
  });

  it("marks an unconfirmed student as not marked only after closure", () => {
    expect(
      attendanceViewState({ periodoAbierto: true, periodoCerrado: true, estado: null }),
    ).toEqual("expired-unconfirmed");
  });
});
