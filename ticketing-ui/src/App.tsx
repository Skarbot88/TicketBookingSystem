import { useEffect, useCallback, useState } from "react";
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  Container,
  Divider,
  Grow,
  LinearProgress,
  Paper,
  Snackbar,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import SportsEsportsIcon from "@mui/icons-material/SportsEsports";
import ConfirmationNumberIcon from "@mui/icons-material/ConfirmationNumber";
import BoltIcon from "@mui/icons-material/Bolt";
import GroupsIcon from "@mui/icons-material/Groups";
import LocalFireDepartmentIcon from "@mui/icons-material/LocalFireDepartment";
import EmojiEventsIcon from "@mui/icons-material/EmojiEvents";
import { getEvent, reserveTicket, purchaseTicket, ApiError } from "./api";
import type { EventDetails } from "./types/types";
import { neon } from "./theme";

const EVENT_ID = 1;

type ReservationState = {
  ticketId: number;
  holderName: string;
} | null;

type Toast = { severity: "success" | "error" | "info"; text: string } | null;

export default function App() {
  const [event, setEvent] = useState<EventDetails | null>(null);
  const [reservation, setReservation] = useState<ReservationState>(null);
  const [holderNameInput, setHolderNameInput] = useState("");
  const [toast, setToast] = useState<Toast>(null);
  const [toastTick, setToastTick] = useState(0);
  const [isBusy, setIsBusy] = useState(false);

  const notify = useCallback((t: Toast) => {
    setToast(t);
    setToastTick((n) => n + 1);
  }, []);

  const refreshEvent = useCallback(async () => {
    const data = await getEvent(EVENT_ID);
    setEvent(data);
  }, []);

  useEffect(() => {
    refreshEvent().catch((err) => notify({ severity: "error", text: describeError(err) }));
  }, [refreshEvent, notify]);

  async function handleReserve() {
    if (!holderNameInput.trim()) {
      notify({ severity: "error", text: "Enter your name first." });
      return;
    }
    setIsBusy(true);
    try {
      const result = await reserveTicket(EVENT_ID, holderNameInput.trim());
      setReservation({ ticketId: result.ticketId, holderName: holderNameInput.trim() });
      notify({ severity: "success", text: `Reserved ticket #${result.ticketId}. You have 10 minutes to purchase.` });
      await refreshEvent();
    } catch (err) {
      notify({ severity: "error", text: describeError(err) });
    } finally {
      setIsBusy(false);
    }
  }

  async function handlePurchase() {
    if (!reservation) return;
    setIsBusy(true);
    try {
      await purchaseTicket(reservation.ticketId, reservation.holderName);
      notify({ severity: "success", text: `Purchase complete for ticket #${reservation.ticketId}.` });
      setReservation(null);
      await refreshEvent();
    } catch (err) {
      notify({ severity: "error", text: describeError(err) });
    } finally {
      setIsBusy(false);
    }
  }

  function describeError(err: unknown): string {
    if (err instanceof ApiError) {
      if (err.status === 409) return "Sorry — that ticket is no longer available.";
      if (err.status === 400) return err.message || "Invalid request.";
      if (err.status === 404) return "Event or ticket not found.";
      return `Something went wrong (${err.status}).`;
    }
    return "Network error — check your connection and try again.";
  }

  if (!event) {
    return (
      <Box sx={{ display: "flex", flexDirection: "column", alignItems: "center", justifyContent: "center", minHeight: "100svh", gap: 2 }}>
        <CircularProgress sx={{ color: neon.cyan, filter: `drop-shadow(0 0 8px ${neon.cyan})` }} />
        <Typography variant="h6" color="text.secondary">
          {toast?.text ?? "Loading event…"}
        </Typography>
      </Box>
    );
  }

  const soldRatio = event.availableCount + event.reservedCount + event.soldCount > 0
    ? (event.soldCount / (event.availableCount + event.reservedCount + event.soldCount)) * 100
    : 0;

  return (
    <Box sx={{ minHeight: "100svh", display: "flex", alignItems: "center", justifyContent: "center", py: 6 }}>
      <Container maxWidth="sm">
        <Stack spacing={1} sx={{ mb: 3, alignItems: "center" }}>
          <SportsEsportsIcon sx={{ fontSize: 40, color: neon.magenta, animation: "pulse-glow 2.4s ease-in-out infinite" }} />
          <Typography
            variant="h4"
            component="h1"
            align="center"
            sx={{
              backgroundImage: `linear-gradient(90deg, ${neon.cyan}, ${neon.magenta})`,
              backgroundClip: "text",
              WebkitBackgroundClip: "text",
              color: "transparent",
              textShadow: `0 0 30px ${neon.magenta}33`,
            }}
          >
            {event.name}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Starts: {new Date(event.startsAt).toLocaleString()}
          </Typography>
        </Stack>

        <Paper
          elevation={0}
          sx={{
            position: "relative",
            p: 4,
            border: `1px solid ${neon.cyan}44`,
            boxShadow: `0 0 30px ${neon.cyan}22, inset 0 0 40px ${neon.magenta}0d`,
            overflow: "hidden",
          }}
        >
          <Stack
            direction="row"
            spacing={1.5}
            useFlexGap
            sx={{ mb: 3, justifyContent: "center", flexWrap: "wrap" }}
          >
            <Chip
              icon={<BoltIcon />}
              label={`Available: ${event.availableCount}`}
              sx={{ color: neon.green, borderColor: neon.green, border: "1px solid", bgcolor: `${neon.green}14` }}
              variant="outlined"
            />
            <Chip
              icon={<GroupsIcon />}
              label={`Reserved: ${event.reservedCount}`}
              sx={{ color: neon.amber, borderColor: neon.amber, border: "1px solid", bgcolor: `${neon.amber}14` }}
              variant="outlined"
            />
            <Chip
              icon={<ConfirmationNumberIcon />}
              label={`Sold: ${event.soldCount}`}
              sx={{ color: neon.magenta, borderColor: neon.magenta, border: "1px solid", bgcolor: `${neon.magenta}14` }}
              variant="outlined"
            />
          </Stack>

          <LinearProgress
            variant="determinate"
            value={soldRatio}
            sx={{
              height: 8,
              borderRadius: 4,
              mb: 3,
              bgcolor: "rgba(255,255,255,0.06)",
              "& .MuiLinearProgress-bar": {
                borderRadius: 4,
                backgroundImage: `linear-gradient(90deg, ${neon.cyan}, ${neon.magenta})`,
                boxShadow: `0 0 10px ${neon.magenta}aa`,
              },
            }}
          />

          <Divider sx={{ mb: 3, borderColor: "rgba(255,255,255,0.08)" }} />

          {!reservation && (
            <Grow in timeout={400}>
              <Stack spacing={2} sx={{ alignItems: "center" }}>
                <TextField
                  label="Your name"
                  value={holderNameInput}
                  onChange={(e) => setHolderNameInput(e.target.value)}
                  disabled={isBusy}
                  fullWidth
                  variant="outlined"
                  onKeyDown={(e) => {
                    if (e.key === "Enter") handleReserve();
                  }}
                />
                <Button
                  onClick={handleReserve}
                  disabled={isBusy || event.availableCount === 0}
                  variant="contained"
                  color="primary"
                  size="large"
                  fullWidth
                  startIcon={isBusy ? <CircularProgress size={18} color="inherit" /> : <LocalFireDepartmentIcon />}
                >
                  {event.availableCount === 0 ? "Sold out" : "Reserve ticket"}
                </Button>
              </Stack>
            </Grow>
          )}

          {reservation && (
            <Grow in timeout={400}>
              <Stack spacing={2} sx={{ alignItems: "center" }}>
                <Typography variant="body1" align="center">
                  Ticket <strong style={{ color: neon.cyan }}>#{reservation.ticketId}</strong> reserved for{" "}
                  <strong>{reservation.holderName}</strong>.
                </Typography>
                <Button
                  onClick={handlePurchase}
                  disabled={isBusy}
                  variant="contained"
                  color="secondary"
                  size="large"
                  fullWidth
                  startIcon={isBusy ? <CircularProgress size={18} color="inherit" /> : <EmojiEventsIcon />}
                >
                  Complete purchase
                </Button>
              </Stack>
            </Grow>
          )}
        </Paper>
      </Container>

      <Snackbar
        open={!!toast}
        autoHideDuration={4000}
        onClose={() => setToast(null)}
        anchorOrigin={{ vertical: "bottom", horizontal: "center" }}
      >
        <Alert
          key={toastTick}
          severity={toast?.severity ?? "info"}
          variant="filled"
          onClose={() => setToast(null)}
          sx={{
            animation: toast?.severity === "error" ? "shake 0.4s ease" : "pop-in 0.35s ease",
            boxShadow:
              toast?.severity === "error"
                ? `0 0 16px ${neon.red}aa`
                : toast?.severity === "success"
                ? `0 0 16px ${neon.green}aa`
                : undefined,
          }}
        >
          {toast?.text}
        </Alert>
      </Snackbar>
    </Box>
  );
}
