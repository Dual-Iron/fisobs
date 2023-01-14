namespace Fisobs.Saves
{
    enum WriteError { None, IOError, TooLong }
    enum ReadError { None, IOError, TooShort, TooLong, InvalidFormat, FutureVersion, TamperedWith }

    readonly struct ReadResult
    {
        public readonly FisobSave? success;
        public readonly ReadError err;

        private ReadResult(FisobSave? success, ReadError err)
        {
            this.success = success;
            this.err = err;
        }

        public static implicit operator ReadResult(FisobSave success) => new(success, default);
        public static implicit operator ReadResult(ReadError error) => new(null, error);
    }
}
