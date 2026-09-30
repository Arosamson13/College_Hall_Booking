const express = require('express');
const path = require('path');

const app = express();
const PORT = process.env.PORT || 5000;

app.use(express.json());
app.use(express.urlencoded({ extended: true }));
app.use(express.static(path.join(__dirname, 'wwwroot')));

// In-memory state for live preview interactive demo
let halls = [
  {
    id: 1,
    name: "Main Auditorium",
    location: "Academic Block A - 1st Floor",
    capacity: 600,
    facilities: "HD Projector, Centralized AC, Dolby Audio, Stage Lighting, Wireless Mics",
    description: "Grand auditorium suitable for college convocations, cultural festivals, inter-college events, and guest seminars.",
    imagePath: "https://images.unsplash.com/photo-1517245386807-bb43f82c33c4?auto=format&fit=crop&w=600&q=80",
    isActive: true
  },
  {
    id: 2,
    name: "APJ Abdul Kalam Seminar Hall",
    location: "Science & Tech Building - 2nd Floor",
    capacity: 250,
    facilities: "Smart Board, AC, Video Conferencing, Surround Speakers, Wi-Fi",
    description: "State-of-the-art seminar hall equipped for technical workshops, department symposiums, and guest lectures.",
    imagePath: "https://images.unsplash.com/photo-1540575467063-178a50c2df87?auto=format&fit=crop&w=600&q=80",
    isActive: true
  },
  {
    id: 3,
    name: "Sir CV Raman Mini Conference Hall",
    location: "Administrative Block - Ground Floor",
    capacity: 80,
    facilities: "LED Display Screen, AC, Modular Conference Table, Executive Chairs",
    description: "Ideal for faculty meetings, placement interviews, department board meetings, and small group presentations.",
    imagePath: "https://images.unsplash.com/photo-1431540015161-0bf868a2d407?auto=format&fit=crop&w=600&q=80",
    isActive: true
  },
  {
    id: 4,
    name: "Open Air Theatre (OAT)",
    location: "Central Campus Grounds",
    capacity: 1500,
    facilities: "Outdoor Stage, High-Power PA System, Flood Lighting",
    description: "Spacious outdoor venue for annual cultural fests, music concerts, and sports ceremony events.",
    imagePath: "https://images.unsplash.com/photo-1470225620780-dba8ba36b745?auto=format&fit=crop&w=600&q=80",
    isActive: true
  }
];

let departments = [
  { id: 1, code: "CSE", name: "Computer Science & Engineering", email: "cse@college.edu", phone: "+91 9876543210", head: "Dr. Alan Turing", isActive: true },
  { id: 2, code: "ECE", name: "Electronics & Communication", email: "ece@college.edu", phone: "+91 9876543211", head: "Dr. Homi Bhabha", isActive: true },
  { id: 3, code: "MECH", name: "Mechanical Engineering", email: "mech@college.edu", phone: "+91 9876543212", head: "Dr. Nikola Tesla", isActive: true },
  { id: 4, code: "MBA", name: "School of Management", email: "mba@college.edu", phone: "+91 9876543213", head: "Dr. Peter Drucker", isActive: true }
];

let bookings = [
  {
    id: 101,
    hallId: 1,
    departmentId: 1,
    hallName: "Main Auditorium",
    deptCode: "CSE",
    deptName: "Computer Science & Engineering",
    bookingDate: "2026-10-05",
    startTime: "10:00",
    endTime: "13:00",
    purpose: "National Hackathon 2026 Opening Ceremony",
    expectedParticipants: 450,
    status: "Approved",
    adminRemarks: "Approved by Admin. Sound system technician assigned.",
    createdAt: "2026-09-29T10:00:00Z"
  },
  {
    id: 102,
    hallId: 2,
    departmentId: 2,
    hallName: "APJ Abdul Kalam Seminar Hall",
    deptCode: "ECE",
    deptName: "Electronics & Communication",
    bookingDate: "2026-10-06",
    startTime: "14:00",
    endTime: "17:00",
    purpose: "VLSI Workshop & Hands-on Lab Session",
    expectedParticipants: 180,
    status: "Pending",
    adminRemarks: "",
    createdAt: "2026-09-30T09:15:00Z"
  },
  {
    id: 103,
    hallId: 3,
    departmentId: 4,
    hallName: "Sir CV Raman Mini Conference Hall",
    deptCode: "MBA",
    deptName: "School of Management",
    bookingDate: "2026-10-08",
    startTime: "11:00",
    endTime: "13:00",
    purpose: "Campus Recruitment Strategy Meeting",
    expectedParticipants: 45,
    status: "Pending",
    adminRemarks: "",
    createdAt: "2026-09-30T14:20:00Z"
  }
];

function checkOverlap(hallId, bookingDate, startTime, endTime, excludeId = null) {
  return bookings.some(b => {
    if (b.id === excludeId) return false;
    if (b.hallId == hallId && b.bookingDate === bookingDate && b.status === 'Approved') {
      return (startTime < b.endTime && endTime > b.startTime);
    }
    return false;
  });
}

// API Routes
app.get('/api/halls', (req, res) => res.json(halls));
app.get('/api/departments', (req, res) => res.json(departments));
app.get('/api/bookings', (req, res) => res.json(bookings));

app.get('/api/Calendar/events', (req, res) => {
  const { hallId } = req.query;
  let filtered = bookings.filter(b => b.status === 'Approved' || b.status === 'Pending');
  if (hallId) {
    filtered = filtered.filter(b => b.hallId == hallId);
  }
  const events = filtered.map(b => ({
    id: b.id,
    title: `${b.deptCode}: ${b.purpose} (${b.hallName})`,
    start: `${b.bookingDate}T${b.startTime}:00`,
    end: `${b.bookingDate}T${b.endTime}:00`,
    color: b.status === 'Approved' ? '#10B981' : '#F59E0B',
    extendedProps: {
      status: b.status,
      hallName: b.hallName,
      deptName: b.deptName
    }
  }));
  res.json(events);
});

app.post('/api/bookings/create', (req, res) => {
  const { hallId, deptId, bookingDate, startTime, endTime, purpose, expectedParticipants } = req.body;

  if (startTime >= endTime) {
    return res.status(400).json({ success: false, message: "End time must be after start time." });
  }

  const hall = halls.find(h => h.id == hallId);
  if (!hall || !hall.isActive) {
    return res.status(400).json({ success: false, message: "Selected hall is inactive or invalid." });
  }

  if (parseInt(expectedParticipants) > hall.capacity) {
    return res.status(400).json({ success: false, message: `Participants exceed hall capacity (${hall.capacity}).` });
  }

  if (checkOverlap(hallId, bookingDate, startTime, endTime)) {
    return res.status(400).json({ success: false, message: "The selected hall is already reserved during this time slot." });
  }

  const dept = departments.find(d => d.id == deptId) || departments[0];

  const newBooking = {
    id: Date.now(),
    hallId: parseInt(hallId),
    departmentId: dept.id,
    hallName: hall.name,
    deptCode: dept.code,
    deptName: dept.name,
    bookingDate,
    startTime,
    endTime,
    purpose,
    expectedParticipants: parseInt(expectedParticipants),
    status: "Pending",
    adminRemarks: "",
    createdAt: new Date().toISOString()
  };

  bookings.unshift(newBooking);
  res.json({ success: true, message: "Booking request submitted successfully!", booking: newBooking });
});

app.post('/api/bookings/approve', (req, res) => {
  const { id, remarks } = req.body;
  const booking = bookings.find(b => b.id == id);
  if (!booking) return res.status(404).json({ success: false, message: "Booking not found." });

  if (checkOverlap(booking.hallId, booking.bookingDate, booking.startTime, booking.endTime, booking.id)) {
    return res.status(400).json({ success: false, message: "Cannot approve: Overlapping approved booking exists." });
  }

  booking.status = "Approved";
  booking.adminRemarks = remarks || "Approved by Admin.";
  res.json({ success: true, message: "Booking approved successfully!" });
});

app.post('/api/bookings/reject', (req, res) => {
  const { id, remarks } = req.body;
  const booking = bookings.find(b => b.id == id);
  if (!booking) return res.status(404).json({ success: false, message: "Booking not found." });

  booking.status = "Rejected";
  booking.adminRemarks = remarks || "Rejected by Admin.";
  res.json({ success: true, message: "Booking request rejected." });
});

app.post('/api/halls/create', (req, res) => {
  const { name, location, capacity, facilities, description, imagePath } = req.body;
  const newHall = {
    id: Date.now(),
    name,
    location,
    capacity: parseInt(capacity),
    facilities,
    description,
    imagePath: imagePath || "https://images.unsplash.com/photo-1517245386807-bb43f82c33c4?auto=format&fit=crop&w=600&q=80",
    isActive: true
  };
  halls.push(newHall);
  res.json({ success: true, hall: newHall });
});

app.post('/api/departments/create', (req, res) => {
  const { code, name, email, phone, head, initialPassword } = req.body;
  const newDept = {
    id: Date.now(),
    code: code.toUpperCase(),
    name,
    email,
    phone,
    head,
    isActive: true
  };
  departments.push(newDept);
  res.json({ success: true, department: newDept, tempPassword: initialPassword || "Dept@123456" });
});

app.get('*', (req, res) => {
  res.sendFile(path.join(__dirname, 'wwwroot', 'index.html'));
});

app.listen(PORT, '0.0.0.0', () => {
  console.log(`Server running on http://0.0.0.0:${PORT}`);
});
